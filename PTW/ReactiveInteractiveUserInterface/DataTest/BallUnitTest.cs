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
    public class BallUnitTest
    {
        [TestMethod]
        public void ConstructorTestMethod()
        {
            Vector testinVector = new Vector(0.0, 0.0);
            Ball newInstance = new(testinVector, testinVector);
            Assert.AreEqual(testinVector, newInstance.Velocity);
        }

        [TestMethod]
        public void MoveTestMethod()
        {
            Vector initialPosition = new(10.0, 10.0);
            Ball newInstance = new(initialPosition, new Vector(0.0, 0.0));
            IVector curentPosition = new Vector(0.0, 0.0);
            int numberOfCallBackCalled = 0;
            newInstance.NewPositionNotification += (sender, position) => { Assert.IsNotNull(sender); curentPosition = position; numberOfCallBackCalled++; };
            newInstance.Move(new Vector(0.0, 0.0));
            Assert.AreEqual<int>(1, numberOfCallBackCalled);
            Assert.AreEqual<IVector>(initialPosition, curentPosition);
        }

        [TestMethod]
        public void MoveWithVelocityTestMethod()
        {
            Vector initialPosition = new(10.0, 10.0);
            Vector velocity = new(5.0, 5.0);
            Ball newInstance = new(initialPosition, velocity);


            IVector initialVelocity = newInstance.Velocity;


            newInstance.Move(new Vector(1.0, 1.0));


            Assert.AreEqual(initialVelocity.x, newInstance.Velocity.x);
            Assert.AreEqual(initialVelocity.y, newInstance.Velocity.y);


        }

        [TestMethod]
        public void ConcurrentVelocityTest()
        {

            Vector initialPosition = new Vector(0.0, 0.0);
            Vector initialVelocity = new Vector(1.0, 1.0);
            Ball ball = new(initialPosition, initialVelocity);
            int taskCount = 100;
            var tasks = new List<Task>();


            for (int i = 0; i < taskCount; i++)
            {
                int index = i;
                tasks.Add(Task.Run(() => {
                    if (index % 2 == 0)
                        ball.Velocity = new Vector(index, index);
                    else
                    {
                        IVector velocity = ball.Velocity;
                        Assert.IsNotNull(velocity);
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray());

            Assert.IsTrue(ball.Velocity.x >= 0);
            Assert.IsTrue(ball.Velocity.y >= 0);
        }

        [TestMethod]
        public void BallTimeTest()
        {
            Vector initialPosition = new Vector(0.0, 0.0);
            Vector initialVelocity = new Vector(1.0, 0.0);
            Ball ball = new Ball(initialPosition, initialVelocity);

            Vector lastPosition = new Vector(0.0, 0.0);
            AutoResetEvent positionUpdated = new AutoResetEvent(false);

            ball.NewPositionNotification += (sender, pos) =>
            {
                lastPosition = (Vector)pos;
                positionUpdated.Set();
            };

            ball.SetSpeedFactor(30); 
            ball.Start();

            
            bool updated = positionUpdated.WaitOne(100);
            ball.Stop();

            Assert.IsTrue(updated);
            Assert.IsTrue(lastPosition.x > 0);
        }

    }
}