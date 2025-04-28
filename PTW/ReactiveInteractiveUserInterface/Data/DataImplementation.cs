//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using System;
using System.Diagnostics;

namespace TP.ConcurrentProgramming.Data
{
    internal class DataImplementation : DataAbstractAPI
    {

        private readonly object ballsListLock = new object();
        private readonly object speedFactorLock = new object();
        private List<Ball> BallsList = [];
        private double speedFactor = 0.0275;

        #region ctor

        public DataImplementation()
        {
            MoveTimer = new Timer(Move, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(1000 / 65));

        }

        #endregion ctor

        #region DataAbstractAPI

        public override void Start(int numberOfBalls, Action<IVector, IBall> upperLayerHandler)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataImplementation));
            if (upperLayerHandler == null)
                throw new ArgumentNullException(nameof(upperLayerHandler));
            Random random = new Random();

            lock (ballsListLock)
            {
                for (int i = 0; i < numberOfBalls; i++)
                {
                    Vector startingPosition = new(random.Next(100, 400 - 100), random.Next(100, 400 - 100));
                    Ball newBall = new(startingPosition, startingPosition);
                    upperLayerHandler(startingPosition, newBall);
                    BallsList.Add(newBall);
                }
            }
        }

        #endregion DataAbstractAPI

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    MoveTimer.Dispose();
                    BallsList.Clear();
                }
                Disposed = true;
            }
            else
                throw new ObjectDisposedException(nameof(DataImplementation));
        }

        public override void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable

        #region private

        //private bool disposedValue;
        private bool Disposed = false;

        private readonly Timer MoveTimer;
        private Random RandomGenerator = new();


        public override void UpdateSpeed(double newSpeed)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataImplementation));

            lock (speedFactorLock)
            {
                speedFactor = (newSpeed - 1) / 9 * (0.05 - 0.005) + 0.005;
            }
        }

        private void Move(object? x)
        {
            if (Disposed) return;

            List<Ball> ballsCopy;
            double currentSpeedFactor;

            lock (ballsListLock)
            {
                ballsCopy = new List<Ball>(BallsList);
            }

            lock (speedFactorLock)
            {
                currentSpeedFactor = speedFactor;
            }

            foreach (Ball item in ballsCopy)
            {
                Vector velocity;
                lock (item) 
                {
                    velocity = (Vector)item.Velocity;
                }

                Vector delta = new Vector(velocity.x * currentSpeedFactor, velocity.y * currentSpeedFactor);

                item.Move(delta);
            }
        }

        #endregion private

        #region TestingInfrastructure

        [Conditional("DEBUG")]
        internal void CheckBallsList(Action<IEnumerable<IBall>> returnBallsList)
        {
            returnBallsList(BallsList);
        }

        [Conditional("DEBUG")]
        internal void CheckNumberOfBalls(Action<int> returnNumberOfBalls)
        {
            returnNumberOfBalls(BallsList.Count);
        }

        [Conditional("DEBUG")]
        internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
        {
            returnInstanceDisposed(Disposed);
        }

        [Conditional("DEBUG")]
        internal void CheckSpeedFactor(Action<double> returnSpeedFactor)
        {
            returnSpeedFactor(speedFactor);
        }


        #endregion TestingInfrastructure
    }
}