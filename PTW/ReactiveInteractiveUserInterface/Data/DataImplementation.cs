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
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace TP.ConcurrentProgramming.Data
{
    internal class DataImplementation : DataAbstractAPI
    {

        private readonly object ballsListLock = new object();
        private readonly object speedFactorLock = new object();
        private readonly ConcurrentQueue<string> logBuffer = new();
        private List<Ball> BallsList = [];
        private double speedFactor = 0.0275;
        private Timer? addBallTimer;
        private Timer? logFlushTimer;
        private Action<IVector, IBall> upperLayerHandler;



        #region ctor

        public DataImplementation()
        {

        }

        #endregion ctor

        #region DataAbstractAPI

        //public override void Start(int numberOfBalls, Action<IVector, IBall> upperLayerHandler)
        //{
        //    if (Disposed)
        //        throw new ObjectDisposedException(nameof(DataImplementation));
        //    if (upperLayerHandler == null)
        //        throw new ArgumentNullException(nameof(upperLayerHandler));



        //    this.upperLayerHandler = upperLayerHandler;
        //    Random random = new Random();


        //    lock (ballsListLock)
        //    {
        //        for (int i = 0; i < numberOfBalls; i++)
        //        {
        //            Vector startingPosition = new(random.Next(100, 400 - 100), random.Next(100, 400 - 100));
        //            Vector initialVelocity = new Vector((random.NextDouble() * 2 - 1) * 2, (random.NextDouble() * 2 - 1) * 2);
        //            Ball newBall = new(startingPosition, initialVelocity);
        //            newBall.SetSpeedFactor(speedFactor);
        //            newBall.Start();
        //            upperLayerHandler(startingPosition, newBall);
        //            BallsList.Add(newBall);
        //        }
        //    }

        //    addBallTimer = new Timer(AddNewBall, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
        //}
        public override void Start(int numberOfBalls, Action<IVector, IBall> upperLayerHandler)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataImplementation));
            if (upperLayerHandler == null)
                throw new ArgumentNullException(nameof(upperLayerHandler));

            addBallTimer?.Dispose();
            logFlushTimer?.Dispose();
            this.upperLayerHandler = upperLayerHandler;

            lock (ballsListLock)
            {
                BallsList.Clear();
                for (int i = 0; i < numberOfBalls; i++)
                {
                    AddSingleBall();

                }
            }
            addBallTimer = new Timer(_ => AddSingleBall(), null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
            logFlushTimer = new Timer(_ => FlushLogsToFile(), null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));


        }

        private void AddSingleBall()
        {
            try
            {
                Vector position = new(RandomGenerator.Next(100, 300), RandomGenerator.Next(100, 300));
                Vector velocity = new((RandomGenerator.NextDouble() * 2 - 1) * 2, (RandomGenerator.NextDouble() * 2 - 1) * 2);
                Ball newBall = new(position, velocity);
                newBall.SetSpeedFactor(speedFactor);

                lock (ballsListLock)
                {
                    BallsList.Add(newBall);
                }

                newBall.Start();

                logBuffer.Enqueue($"{DateTime.Now:HH:mm:ss} - Dodano kulke na pozycji ({position.x}, {position.y})");
                upperLayerHandler?.Invoke(position, newBall);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adding ball: {ex}");
            }
        }

        private void FlushLogsToFile()
        {
            try
            {
                string logPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ball_log.txt");

                using StreamWriter writer = new(logPath, append: true);

                int linesWritten = 0;

                int ballsAddedInThisFlush = 0;

                while (logBuffer.TryDequeue(out var line))
                {
                    writer.WriteLine(line);
                    linesWritten++;
                    ballsAddedInThisFlush++;
                }

                if (ballsAddedInThisFlush > 0)
                {
                    Debug.WriteLine($"Zapisano do pliku {ballsAddedInThisFlush} pilek w tej operacji.");
                    writer.WriteLine($"Zapisano {ballsAddedInThisFlush} pilek w tym zapisie: {DateTime.Now:HH:mm:ss}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Log writing error: {ex}");
            }
        }
        


        public override void UpdateSpeed(double newSpeed)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataImplementation));

            lock (speedFactorLock)
            {
                speedFactor = 0.5 + (newSpeed - 1) * (2.0 - 0.5) / 9;

            }

            lock (ballsListLock)
            {
                foreach (var ball in BallsList)
                {
                    ball.SetSpeedFactor(speedFactor);
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
                    addBallTimer?.Dispose();
                    addBallTimer = null;
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