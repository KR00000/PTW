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
    #region ctor

    internal Ball(Vector initialPosition, Vector initialVelocity)
    {
      Position = initialPosition;
      Velocity = initialVelocity;
    }

    #endregion ctor

    #region IBall

    public event EventHandler<IVector>? NewPositionNotification;

    public IVector Velocity { get; set; }

    #endregion IBall

    #region private

    private Vector Position;

    private void RaiseNewPositionChangeNotification()
    {
      NewPositionNotification?.Invoke(this, Position);
    }

    internal void Move(Vector delta)
    {
         
            double newX = Position.x + delta.x;
            double newY = Position.y + delta.y;

            // Wymiary planszy
            double width = 382; 
            double height = 402; 
            double ballSize = 10.0; 

            // Odbicie od ścianek (lewa/prawa)
            if (newX < 0)
            {
                Velocity = new Vector(-Velocity.x, Velocity.y); // Odbicie w poziomie (zmiana prędkości w osi X)
                newX = 0; // Ustawienie piłki przy lewej krawędzi
            }
            else if (newX > width - ballSize)
            {
                Velocity = new Vector(-Velocity.x, Velocity.y); // Odbicie w poziomie (zmiana prędkości w osi X)
                newX = width - ballSize; // Ustawienie piłki przy prawej krawędzi
            }

            // Odbicie od ścianek (góra/dół)
            if (newY < 0)
            {
                Velocity = new Vector(Velocity.x, -Velocity.y); // Odbicie w pionie (zmiana prędkości w osi Y)
                newY = 0; // Ustawienie piłki przy górnej krawędzi
            }
            else if (newY > height - ballSize)
            {
                Velocity = new Vector(Velocity.x, -Velocity.y); // Odbicie w pionie (zmiana prędkości w osi Y)
                newY = height - ballSize; // Ustawienie piłki przy dolnej krawędzi
            }

            // Ustawienie nowej pozycji piłki
            Position = new Vector(newX, newY);
            RaiseNewPositionChangeNotification();
        }

    #endregion private
  }
}