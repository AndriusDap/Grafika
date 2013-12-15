using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GrafikosEfektųProgramavimas
{
   
    class CameraControl
    {
        
	static public Vector3 CameraResult; //This Vector3 holds the general camera movements
	static public Vector3 LookAtResult; //This Vector3 holds the movements of the LookAt and only them.
	/// <summary>
	/// The standard camera movements (Output: Vector3 CameraResut / Vector3 LookAtResult)
	/// </summary>
	/// <param name="CameraPosition">Current camera position</param>
	/// <param name="LookAt">Current lookat position</param>
	/// <param name="Speed">Velocity of the camera movements</param>
	static public void Hover(Vector3 CameraPosition, Vector3 LookAt, float Speed)
	{
        //http://www.xnawiki.com/index.php/Easy_Camera_Movement_Class
		KeyboardState keyboard = Keyboard.GetState();
		MouseState mouse = Mouse.GetState();
 
		//Create all needed values:
		#region Create Values
 
		//The direction from the Camera to the LookAt:
		Vector3 Direction = LookAt - CameraPosition;
 
		Direction.Normalize();
 
		//This Vector3 defines the relative X-axis of the view (Forward).
		Vector3 RelativeX = Direction;
 
		//AlphaY holds the rotation of RelativeX around the absolute Y-axis, starting at the absolute X-axis.
		float AlphaY = 0.0f;
 
		if (RelativeX.Z >= 0)
		{
			AlphaY = (float)Math.Atan2(RelativeX.Z, RelativeX.X);
		}
		else if (RelativeX.Z < 0)
		{
			AlphaY = (float)Math.Atan2(RelativeX.Z, RelativeX.X) + (2 * (float)Math.PI);
		}
 
		//AlphaZ holds the rotation of RelativeX around the RelativeZ axis (Right).
		//RelativeZ will be defined later, based on RelativeX.
		float AlphaZ = -(float)Math.Atan(RelativeX.Y /
		(float)Math.Sqrt(Math.Pow(RelativeX.X, 2f) + Math.Pow(RelativeX.Z, 2f)));
		//The RelativeZ axis holds the driection Right. It will be used for movements.
		Vector3 RelativeZ;
		RelativeZ.X = RelativeX.Z;
		RelativeZ.Y = 0.0f;
		RelativeZ.Z = -RelativeX.X;
 
		RelativeZ.Normalize();
 
		//The RelativeY axis holds the direction Up. Again it will be used for movements.
		Vector3 RelativeY = new Vector3(0, 1, 0);
 
		RelativeY.X = -RelativeX.Y;
		RelativeY.Y = (float)Math.Sqrt(Math.Pow(RelativeX.X, 2f) + Math.Pow(RelativeX.Z, 2f));
 
		RelativeY = Vector3.Transform(RelativeY, Matrix.CreateRotationY(-AlphaY));
 
		RelativeY.Normalize();
 
		//Two integers to indicate who far the mouse has moved since the last fram:
		float mousemoveX = 0;
		float mousemoveY = 0;
 
		// This float holds the Angle the camera has moved around the Z.axis since last frame.
		float AngleAddZ = 0;
 
		// This float holds the Angle the camera has moved around the Y.axis since last frame.
		float AngleAddY = 0;
 
		//This Vektor holds the relative position of the LookAt based on the CameraPosition,
		//It will be importent to calculate the new LookAt position.
		Vector3 Before = CameraPosition - LookAt;
 
		//After is the vector holding the new relative position of the LookAt.
		Vector3 After = Vector3.Zero;
 
		//Velocity holds the movement of the Camera and the LookAt in the absolute space.
		Vector3 Velocity = Vector3.Zero;
 
		//This Vector will hold the movement of the LookAt that, when applied, will cause the LookAt to
		//rotate around the camera.
		Vector3 Rotation;
 
		#endregion
 
		//Handle the Input by the Keyboard (A/W/S/D) to move the Camera:
		#region Handle Keyboard
 
		//Handle W/S as a control for the forward movements of the Camera:
		if (keyboard.IsKeyDown(Keys.W) && keyboard.IsKeyUp(Keys.S))
		{
			Velocity += RelativeX * Speed * 5;
		}
		else if (keyboard.IsKeyDown(Keys.S) && keyboard.IsKeyUp(Keys.W))
		{
            Velocity -= RelativeX * Speed * 5;
		}
 
		//Handle A/D as a control for the left/right movements of the Camera:
		if (keyboard.IsKeyDown(Keys.D) && keyboard.IsKeyUp(Keys.A))
		{
            Velocity -= RelativeZ * Speed * 5;
		}
		else if (keyboard.IsKeyDown(Keys.A) && keyboard.IsKeyUp(Keys.D))
		{
            Velocity += RelativeZ * Speed * 5;
		}
 
		//Handle E/Q as a control for the up/down movements of the Camera:
		if (keyboard.IsKeyDown(Keys.E) && keyboard.IsKeyUp(Keys.Q))
		{
            Velocity += RelativeY * Speed * 5;
		}
		else if (keyboard.IsKeyDown(Keys.Q) && keyboard.IsKeyUp(Keys.E))
		{
            Velocity -= RelativeY * Speed * 5;
		}
 
		#endregion
 
		//Handle the Input by the Mouse and the Arrow Keys to rotate the Camera:
		#region Handle Mouse
 
		//For this movement control code we will look at the position of the coursor
		//relative to the center of the screen, so you'll need two values for this MonitorWidth
		// and MonitorHeight.
		//For this example I will take the dimensions 1440 to 900.
 
		int MonitorWidth = 1200;
		int MonitorHeight = 600;
 
		mousemoveX = mouse.Y - (MonitorHeight / 2);
		mousemoveY = mouse.X - (MonitorWidth / 2);
 
		AngleAddZ = (float)MathHelper.ToRadians(mousemoveX / 3);
        
		AngleAddY = -(float)MathHelper.ToRadians(mousemoveY / 3);
 
		//Note: The next call will set the mouse into the center of the screen
		// again, in that case you will have to close the window with Alt + F4.
 
		Mouse.SetPosition(MonitorWidth / 2, MonitorHeight / 2);
 
		//Now applay the mousemovements:

		Matrix RotationMatrix =
			  Matrix.CreateFromAxisAngle(RelativeZ, AngleAddZ) *
			  Matrix.CreateRotationY(AngleAddY);
 
		After = Vector3.Transform(Before, RotationMatrix);
        
		Rotation = Before - After;


 
		#endregion
 
		CameraResult = Velocity;

        LookAtResult = Rotation;
	}
}
}
