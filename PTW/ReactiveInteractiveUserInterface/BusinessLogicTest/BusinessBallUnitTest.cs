//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

namespace TP.ConcurrentProgramming.BusinessLogic.Test
{
    [TestClass]
    public class BallUnitTest
    {
        [TestMethod]
        public void MoveTestMethod()
        {
            DataBallFixture dataBallFixture = new DataBallFixture();
            Ball newInstance = new(dataBallFixture);
            int numberOfCallBackCalled = 0;
            newInstance.NewPositionNotification += (sender, position) => { Assert.IsNotNull(sender); Assert.IsNotNull(position); numberOfCallBackCalled++; };
            dataBallFixture.Move();
            Assert.AreEqual<int>(1, numberOfCallBackCalled);
        }

        [TestMethod]
        public void BoundaryCollisionTestTop()
        {
            DataBallFixture dataBallFixture = new DataBallFixture();
            dataBallFixture.SetVelocity(0.0, 5.0);
            Ball ball = new(dataBallFixture);
            double initialVelocityY = dataBallFixture.Velocity.y;

            dataBallFixture.Move(50.0, -5.0);

            Assert.AreEqual(-initialVelocityY, dataBallFixture.Velocity.y);
        }
        [TestMethod]
        public void BoundaryCollisionTestDown()
        {
            DataBallFixture dataBallFixture = new DataBallFixture();
            dataBallFixture.SetVelocity(0.0, -5.0);
            Ball ball = new(dataBallFixture);
            double initialVelocityY = dataBallFixture.Velocity.y;

            dataBallFixture.Move(50.0, 410.0);

            Assert.AreEqual(-initialVelocityY, dataBallFixture.Velocity.y);
        }

        [TestMethod]
        public void BoundaryCollisionTestLeft()
        {
            DataBallFixture dataBallFixture = new DataBallFixture();
            dataBallFixture.SetVelocity(-5.0, 0.0);
            Ball ball = new(dataBallFixture);
            double initialVelocityY = dataBallFixture.Velocity.y;

            dataBallFixture.Move(-5.0, 50.0);

            Assert.AreEqual(-initialVelocityY, dataBallFixture.Velocity.y);
        }

        [TestMethod]
        public void BoundaryCollisionTestRight()
        {
            DataBallFixture dataBallFixture = new DataBallFixture();
            dataBallFixture.SetVelocity(5.0, 0.0);
            Ball ball = new(dataBallFixture);
            double initialVelocityY = dataBallFixture.Velocity.y;

            dataBallFixture.Move(390.0, 50.0);

            Assert.AreEqual(-initialVelocityY, dataBallFixture.Velocity.y);
        }

        [TestMethod]
        public void BallsBouncingTest()
        {
            DataBallFixture dataBall1 = new DataBallFixture();
            DataBallFixture dataBall2 = new DataBallFixture();

            dataBall1.SetVelocity(5.0, 0.0);
            dataBall2.SetVelocity(-5.0, 0.0);

            Ball ball1 = new(dataBall1);
            Ball ball2 = new(dataBall2);

            dataBall1.Move(50.0, 50.0);
            dataBall2.Move(70.0, 50.0);


            double initialVelocity1 = dataBall1.Velocity.x;
            double initialVelocity2 = dataBall2.Velocity.x;

            dataBall1.Move(65.0, 50.0);
            dataBall2.Move(65.0 + ball1.Radius + ball2.Radius - 1, 50.0);

            ball1.HandleCollision(ball2);

            Assert.AreNotEqual(initialVelocity1, dataBall1.Velocity.x);
            Assert.AreNotEqual(initialVelocity2, dataBall2.Velocity.x);

            Assert.IsTrue(dataBall1.Velocity.x < 0);
            Assert.IsTrue(dataBall2.Velocity.x > 0);

        }


        [TestMethod]
        public void BallsMissBounceTest()
        {
            DataBallFixture dataBall1 = new DataBallFixture();
            DataBallFixture dataBall2 = new DataBallFixture();

            dataBall1.SetVelocity(-5.0, 0.0);
            dataBall2.SetVelocity(5.0, 0.0);

            Ball ball1 = new(dataBall1);
            Ball ball2 = new(dataBall2);

            dataBall1.Move(50.0, 50.0);
            dataBall2.Move(70.0, 50.0);


            double initialVelocity1 = dataBall1.Velocity.x;
            double initialVelocity2 = dataBall2.Velocity.x;

            dataBall1.Move(65.0, 50.0);
            dataBall2.Move(65.0 + ball1.Radius + ball2.Radius - 1, 50.0);

            ball1.HandleCollision(ball2);

            Assert.AreEqual(initialVelocity1, dataBall1.Velocity.x);
            Assert.AreEqual(initialVelocity2, dataBall2.Velocity.x);

        }


        [TestMethod]
        [Timeout(1000)]
        public void TestDeadlock()
        {
            DataBallFixture dataBall1 = new DataBallFixture();
            DataBallFixture dataBall2 = new DataBallFixture();
            Ball ball1 = new(dataBall1);
            Ball ball2 = new(dataBall2);

            dataBall1.Move(50.0, 50.0);
            dataBall2.Move(50.0 + ball1.Radius + ball2.Radius - 1, 50.0);

            var t1 = Task.Run(() => ball1.HandleCollision(ball2));
            var t2 = Task.Run(() => ball2.HandleCollision(ball1));

            Task.WaitAll(t1, t2);
        }




        #region testing instrumentation
        private class DataBallFixture : Data.IBall
        {
            private Data.IVector velocity = new VectorFixture(0.0, 0.0);
            private List<EventHandler<Data.IVector>> subscribers = new List<EventHandler<Data.IVector>>();

            public Data.IVector Velocity
            {
                get => velocity;
                set => velocity = value;
            }

            public event EventHandler<Data.IVector>? NewPositionNotification
            {
                add
                {
                    if (value != null)
                        subscribers.Add(value);
                }
                remove
                {
                    if (value != null)
                        subscribers.Remove(value);
                }
            }

            internal void SetVelocity(double x, double y)
            {
                velocity = new VectorFixture(x, y);
            }

            internal void Move()
            {
                foreach (var subscriber in subscribers)
                {
                    subscriber.Invoke(this, new VectorFixture(0.0, 0.0));
                }
            }

            internal void Move(double x, double y)
            {
                foreach (var subscriber in subscribers)
                {
                    subscriber.Invoke(this, new VectorFixture(x, y));
                }
            }
        }

        private class VectorFixture : Data.IVector
        {
            internal VectorFixture(double X, double Y)
            {
                x = X; y = Y;
            }

            public double x { get; init; }
            public double y { get; init; }
        }

        #endregion testing instrumentation
    }
}