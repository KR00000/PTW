//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

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
        public void UpdateSpeedTestMethod()
        {
            using (DataImplementation newInstance = new DataImplementation())
            {

                newInstance.CheckSpeedFactor(x => Assert.AreEqual(0.0275, x, 0.0001));


                newInstance.UpdateSpeed(7);
                newInstance.CheckSpeedFactor(x => {
                    // 0.005 * 7 = 0.035
                    Assert.IsTrue(x == 0.035);
                });


                newInstance.UpdateSpeed(1);
                newInstance.CheckSpeedFactor(x => Assert.AreEqual(0.005, x, 0.0001));

                newInstance.UpdateSpeed(10);
                newInstance.CheckSpeedFactor(x => Assert.AreEqual(0.05, x, 0.0001));
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

                // Wait for timer to trigger movement
                Thread.Sleep(200);

                // Verify positions have changed
                for (int i = 0; i < ballCount; i++)
                {
                    Assert.AreNotEqual(lastPositions[i].x, 0.0);
                    Assert.AreNotEqual(lastPositions[i].y, 0.0);
                }
            }
        }
    }
}