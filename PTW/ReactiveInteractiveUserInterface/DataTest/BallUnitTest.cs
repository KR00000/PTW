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

        // Test czy pilki po obiciu zmieniaja kierunek
        [TestMethod]
        public void BoundaryCollisionTest()
        {

            Vector position = new(1.0, 200.0);
            Vector velocity = new(-10.0, 0.0);
            Ball ball = new(position, velocity);
            ball.Move(new Vector(-2.0, 0.0));
            Assert.IsTrue(ball.Velocity.x > 0);


            position = new(381.0, 200.0);
            velocity = new(10.0, 0.0);
            ball = new(position, velocity);
            ball.Move(new Vector(2.0, 0.0));
            Assert.IsTrue(ball.Velocity.x < 0);

            position = new(200.0, 1.0);
            velocity = new(0.0, -10.0);
            ball = new(position, velocity);
            ball.Move(new Vector(0.0, -2.0));
            Assert.IsTrue(ball.Velocity.y > 0);

            position = new(200.0, 391.0);
            velocity = new(0.0, 10.0);
            ball = new(position, velocity);
            ball.Move(new Vector(0.0, 2.0));
            Assert.IsTrue(ball.Velocity.y < 0);
        }
    }
}