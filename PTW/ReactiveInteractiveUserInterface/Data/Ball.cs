//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

namespace TP.ConcurrentProgramming.Data
{
    internal class Ball : IBall
    {

        private readonly object stateLock = new object();
        private Vector velocity;


        #region ctor

        internal Ball(Vector initialPosition, Vector initialVelocity)
        {
            Position = initialPosition;
            velocity = initialVelocity;
        }

        #endregion ctor

        #region IBall

        public event EventHandler<IVector>? NewPositionNotification;

        public IVector Velocity
        {
            get
            {
                lock (stateLock)
                {
                    return velocity;
                }
            }
            set
            {
                lock (stateLock)
                {
                    velocity = (Vector)value;
                }
            }
        }

        #endregion IBall

        #region private

        private Vector Position;

        private void RaiseNewPositionChangeNotification()
        {
            Vector positionCopy;


            positionCopy = new Vector(Position.x, Position.y);

            EventHandler<IVector>? handler = NewPositionNotification;
            handler?.Invoke(this, positionCopy);
        }

        internal void Move(Vector delta)
        {


            double newX = Position.x + delta.x;
            double newY = Position.y + delta.y;
            Position = new Vector(newX, newY);
            RaiseNewPositionChangeNotification();

        }

        #endregion private
    }
}