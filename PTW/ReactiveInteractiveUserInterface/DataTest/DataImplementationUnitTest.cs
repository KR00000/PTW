//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using System.Collections.Concurrent;

namespace TP.ConcurrentProgramming.Data.Test
{
    [TestClass]
    public class DataImplementationUnitTest
    {
        [TestMethod]
        public void ConstructorTestMethod()
        {
            using (DataImplementation newInstance = new DataImplementation())
            {
                IEnumerable<IBall>? ballsList = null;
                newInstance.CheckBallsList(x => ballsList = x);
                Assert.IsNotNull(ballsList);
                int numberOfBalls = 0;
                newInstance.CheckNumberOfBalls(x => numberOfBalls = x);
                Assert.AreEqual<int>(0, numberOfBalls);
            }
        }

        [TestMethod]
        public void DisposeTestMethod()
        {
            DataImplementation newInstance = new DataImplementation();
            bool newInstanceDisposed = false;
            newInstance.CheckObjectDisposed(x => newInstanceDisposed = x);
            Assert.IsFalse(newInstanceDisposed);
            newInstance.Dispose();
            newInstance.CheckObjectDisposed(x => newInstanceDisposed = x);
            Assert.IsTrue(newInstanceDisposed);
            IEnumerable<IBall>? ballsList = null;
            newInstance.CheckBallsList(x => ballsList = x);
            Assert.IsNotNull(ballsList);
            newInstance.CheckNumberOfBalls(x => Assert.AreEqual<int>(0, x));
            Assert.ThrowsException<ObjectDisposedException>(() => newInstance.Dispose());
            Assert.ThrowsException<ObjectDisposedException>(() => newInstance.Start(0, (position, ball) => { }));
        }

        [TestMethod]
        public void StartTestMethod()
        {
            using (DataImplementation newInstance = new DataImplementation())
            {
                int numberOfCallbackInvoked = 0;
                int numberOfBalls2Create = 10;
                newInstance.Start(
                  numberOfBalls2Create,
                  (startingPosition, ball) =>
                  {
                      numberOfCallbackInvoked++;
                      Assert.IsTrue(startingPosition.x >= 0);
                      Assert.IsTrue(startingPosition.y >= 0);
                      Assert.IsNotNull(ball);
                  });
                Assert.AreEqual<int>(numberOfBalls2Create, numberOfCallbackInvoked);
                newInstance.CheckNumberOfBalls(x => Assert.AreEqual<int>(10, x));
            }
        }


        [TestMethod]
        public void AddSingleBallTestMethod()
        {
            // Arrange
            using (DataImplementation newInstance = new DataImplementation())
            {
                int callbackInvoked = 0;
                IVector? lastPosition = null;
                IBall? lastBall = null;
                int expectedBalls = 1;

                Action<IVector, IBall> handler = (position, ball) =>
                {
                    callbackInvoked++;
                    lastPosition = position;
                    lastBall = ball;
                };

                newInstance.Start(1, handler);

                newInstance.CheckNumberOfBalls(x => Assert.AreEqual(expectedBalls, x));

                Assert.AreEqual(1, callbackInvoked);
                Assert.IsNotNull(lastPosition);
                Assert.IsNotNull(lastBall);
                Assert.IsTrue(lastPosition.x >= 100 && lastPosition.x <= 300);
                Assert.IsTrue(lastPosition.y >= 100 && lastPosition.y <= 300);

                Assert.IsNotNull(lastBall.Velocity);

                var logBufferField = typeof(DataImplementation).GetField("logBuffer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var logBuffer = (ConcurrentQueue<string>)logBufferField.GetValue(newInstance);
                Assert.IsTrue(logBuffer.TryDequeue(out var logEntry));
                Assert.IsTrue(logEntry.Contains("Dodano kulke na pozycji"));
            }
        }

        [TestMethod]
        public void AddSingleBallMultipleBallsTestMethod()
        {
            using (DataImplementation newInstance = new DataImplementation())
            {
                int numberOfBalls = 5;
                int callbackInvoked = 0;

                newInstance.Start(numberOfBalls, (position, ball) => callbackInvoked++);


                newInstance.CheckNumberOfBalls(x => Assert.AreEqual(numberOfBalls, x));
                Assert.AreEqual(numberOfBalls, callbackInvoked);

                var logBufferField = typeof(DataImplementation).GetField("logBuffer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var logBuffer = (ConcurrentQueue<string>)logBufferField.GetValue(newInstance);
                int logCount = 0;
                while (logBuffer.TryDequeue(out _))
                {
                    logCount++;
                }
                Assert.AreEqual(numberOfBalls, logCount);
            }
        }




        [TestMethod]
        public void UpdateSpeedTestMethod()
        {
            using (DataImplementation newInstance = new DataImplementation())
            {

                newInstance.CheckSpeedFactor(x => Assert.AreEqual(0.0275, x, 0.0001));


                newInstance.UpdateSpeed(7);
                newInstance.CheckSpeedFactor(x => {
                    Assert.IsTrue(x == 1.5);
                });


                newInstance.UpdateSpeed(1);
                newInstance.CheckSpeedFactor(x => Assert.IsTrue(x == 0.5));

                newInstance.UpdateSpeed(10);
                newInstance.CheckSpeedFactor(x => Assert.IsTrue(x == 2));
            }
        }

        [TestMethod]
        public void MoveTimerTest()
        {
            using (DataImplementation newInstance = new DataImplementation())
            {
                int ballCount = 3;
                List<Vector> lastPositions = new List<Vector>();

                newInstance.Start(ballCount, (position, ball) => {
                    lastPositions.Add(new Vector(position.x, position.y));
                    ball.NewPositionNotification += (sender, newPos) => {
                        lastPositions[lastPositions.Count - 1] = new Vector(newPos.x, newPos.y);
                    };
                });

                Thread.Sleep(200);

                for (int i = 0; i < ballCount; i++)
                {
                    Assert.AreNotEqual(lastPositions[i].x, 0.0);
                    Assert.AreNotEqual(lastPositions[i].y, 0.0);
                }
            }
        }
    }
}