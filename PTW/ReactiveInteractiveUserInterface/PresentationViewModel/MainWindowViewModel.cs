﻿//__________________________________________________________________________________________
//
//  Copyright 2024 Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and to get started
//  comment using the discussion panel at
//  https://github.com/mpostol/TP/discussions/182
//__________________________________________________________________________________________

using System;
using System.Collections.ObjectModel;
using System.Threading;

using TP.ConcurrentProgramming.Presentation.Model;
using TP.ConcurrentProgramming.Presentation.ViewModel.MVVMLight;
//using static System.Net.Mime.MediaTypeNames;
using ModelIBall = TP.ConcurrentProgramming.Presentation.Model.IBall;



namespace TP.ConcurrentProgramming.Presentation.ViewModel
{
    public class MainWindowViewModel : ViewModelBase, IDisposable
    {
        #region ctor

        public MainWindowViewModel() : this(null, SynchronizationContext.Current)
        { }

        //internal MainWindowViewModel(ModelAbstractApi modelLayerAPI)
        //{
        //    ModelLayer = modelLayerAPI == null ? ModelAbstractApi.CreateModel() : modelLayerAPI;
        //    Observer = ModelLayer.Subscribe<ModelIBall>(x =>Balls.Add(x));

        //}

        private readonly SynchronizationContext _syncContext;


        public MainWindowViewModel(ModelAbstractApi modelLayerAPI, SynchronizationContext syncContext)
        {
            _syncContext = syncContext ?? SynchronizationContext.Current;
            ModelLayer = modelLayerAPI == null ? ModelAbstractApi.CreateModel() : modelLayerAPI;
            Observer = ModelLayer.Subscribe<ModelIBall>(x =>
            {
                if (_syncContext != null)
                {
                    _syncContext.Post(_ =>
                    {
                        Balls.Add(x);

                    }, null);
                }
                else
                {
                    Balls.Add(x); // Dla testów bez UI

                }
            });
        }


        #endregion ctor

        #region public API

        public void Start(int numberOfBalls)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(MainWindowViewModel));
            ModelLayer.Start(numberOfBalls);
            //Observer.Dispose();
        }

        public ObservableCollection<ModelIBall> Balls { get; } = new ObservableCollection<ModelIBall>();

        #endregion public API

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    Balls.Clear();
                    Observer.Dispose();
                    ModelLayer.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                Disposed = true;
            }
        }

        public void Dispose()
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(MainWindowViewModel));
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable

        #region private

        private IDisposable Observer = null;
        private ModelAbstractApi ModelLayer;
        private bool Disposed = false;

        #endregion private
    }
}