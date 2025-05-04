//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using System.Diagnostics;
using UnderneathLayerAPI = TP.ConcurrentProgramming.Data.DataAbstractAPI;

namespace TP.ConcurrentProgramming.BusinessLogic
{
    internal class BusinessLogicImplementation : BusinessLogicAbstractAPI
    {
        private readonly object ballsLock = new object();
        private List<Ball> balls = new List<Ball>();
        private Timer collisionTimer;
        #region ctor

        public BusinessLogicImplementation() : this(null)
        { }

        internal BusinessLogicImplementation(UnderneathLayerAPI? underneathLayer)
        {
            layerBellow = underneathLayer == null ? UnderneathLayerAPI.GetDataLayer() : underneathLayer;
            collisionTimer = new Timer(DetectCollisions, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(1000.0 / 120));
        }

        #endregion ctor

        #region BusinessLogicAbstractAPI

        public override void Dispose()
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
            collisionTimer?.Dispose();
            layerBellow.Dispose();
            Disposed = true;
        }

        public override void Start(int numberOfBalls, Action<IPosition, IBall> upperLayerHandler)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
            if (upperLayerHandler == null)
                throw new ArgumentNullException(nameof(upperLayerHandler));
            layerBellow.Start(numberOfBalls, (startingPosition, databall) => {
                // Tworzenie obiektu Ball z rozszerzoną logiką odbijania
                var ball = new Ball(databall);
                lock (ballsLock)
                {
                    balls.Add(ball);
                }
                upperLayerHandler(new Position(startingPosition.x, startingPosition.y), ball);
            });
            StartCollisionDetection();
        }


        public override void UpdateSpeed(double newSpeed)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(BusinessLogicImplementation));

            layerBellow.UpdateSpeed(newSpeed);
        }

        #endregion BusinessLogicAbstractAPI

        #region private

        private bool Disposed = false;

        private readonly UnderneathLayerAPI layerBellow;

        private void StartCollisionDetection()
        {
            collisionTimer = new Timer(DetectCollisions, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(1000.0 / 120));
        }

        private async void DetectCollisions(object? state)
        {
            if (Disposed) return;

           
            List<Ball> ballsCopy;
            lock (ballsLock)
            {
                ballsCopy = new List<Ball>(balls);
            }

            var tasks = new List<Task>();

            for (int i = 0; i < ballsCopy.Count; i++)
            {
                for (int j = i + 1; j < ballsCopy.Count; j++)
                {
                    var localI = i;
                    var localJ = j;
                    tasks.Add(Task.Run(() => ProcessCollision(ballsCopy[localI], ballsCopy[localJ])));
                }
            }
            await Task.WhenAll(tasks);
        }


        private void ProcessCollision(Ball ball1, Ball ball2)
        {
            Ball firstBall = ball1.GetHashCode() < ball2.GetHashCode() ? ball1 : ball2;
            Ball secondBall = ball1.GetHashCode() < ball2.GetHashCode() ? ball2 : ball1;

            bool lockTaken1 = false;
            bool lockTaken2 = false;

            try
            {
                Monitor.TryEnter(firstBall, 10, ref lockTaken1);
                if (lockTaken1)
                {
                    Monitor.TryEnter(secondBall, 10, ref lockTaken2);
                    if (lockTaken2)
                    {
                        ball1.HandleCollision(ball2);
                    }
                }
            }
            finally
            {
                if (lockTaken2) Monitor.Exit(secondBall);
                if (lockTaken1) Monitor.Exit(firstBall);
            }
        }
    

        #endregion private

        #region TestingInfrastructure

        [Conditional("DEBUG")]
        internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
        {
            returnInstanceDisposed(Disposed);
        }

        #endregion TestingInfrastructure
    }
}