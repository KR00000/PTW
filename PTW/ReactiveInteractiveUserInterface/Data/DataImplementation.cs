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
using System.Threading.Tasks;

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
                    newBall.Start();
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

                    lock (ballsListLock)
                    {
                        foreach (var ball in BallsList)
                        {
                            ball.Stop(); 
                        }
                        BallsList.Clear();
                    }
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

        private bool Disposed = false;

        private Random RandomGenerator = new();


        public override void UpdateSpeed(double newSpeed)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataImplementation));

            lock (speedFactorLock)
            {
                speedFactor = Math.Min(0.05, Math.Max(0.005, (newSpeed - 1) / 9 * (0.05 - 0.005) + 0.005));
            }

            lock (ballsListLock)
            {
                foreach (var ball in BallsList)
                {
                    ball.SetSpeedFactor(speedFactor);
                }
            }

        }

        //private async void Move(object? x)
        //{
        //    if (Disposed) return;

        //    List<Ball> ballsCopy;
        //    double currentSpeedFactor;

        //    lock (ballsListLock)
        //    {
        //        ballsCopy = new List<Ball>(BallsList);
        //    }

        //    lock (speedFactorLock)
        //    {
        //        currentSpeedFactor = speedFactor;
        //    }

        //    var tasks = new List<Task>();

        //    foreach (var ball in ballsCopy)
        //    {
        //        tasks.Add(Task.Run(() => {
        //            Vector velocity;
        //            lock (ball)
        //            {
        //                velocity = (Vector)ball.Velocity;
        //            }

        //            double speedAdjustment = 60.0 / 60;
        //            Vector delta = new Vector(
        //                velocity.x * currentSpeedFactor * speedAdjustment,
        //                velocity.y * currentSpeedFactor * speedAdjustment
        //            );

        //            ball.Move(delta);
        //        }));
        //    }
        //    await Task.WhenAll(tasks);
        //}
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