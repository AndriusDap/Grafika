using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace GrafikosEfektųProgramavimas
{
    class ButtonMaster
    {
        class Button
        {
            public String Title;
            public Func<String> Function;

            public Button(String Title, Func<String> Function)
            {
                this.Title = Title;
                this.Function = Function;
            }
          
            public void Click()
            {
                if (Function != null)
                {
                    SetTitle(Function());
                }
            }

            public void SetTitle(String NewTitle)
            {
                this.Title = NewTitle;
            }
        }
        public int Height = 0;
        public int Gap = 10;
        public int Width = 0;
        public static Color HighlightedColor = Color.Gray;
        public static Color ButtonColor = Color.White;

        SpriteFont Font;

        List<Button> Buttons;

        Vector2 BoundingTopLeft;
        Vector2 BoundingBottomRight;
        Vector2 ButtonSize;

        MouseState OldMouse;


        int HighlightedId;
        
        public ButtonMaster()
        {
            HighlightedId = -1;
            Buttons = new List<Button>();
            OldMouse = Mouse.GetState();
        }

        public void LoadContent(GraphicsDevice device, ContentManager Content)
        {
            Font = Content.Load<SpriteFont>("Font");
            BoundingTopLeft = new Vector2(Gap, Gap);
            BoundingBottomRight = new Vector2(Gap + Width, Gap);
        }

        public void AddButton(String Title, Func<String> ButtonFunction)
        {
            Buttons.Add(new Button(Title, ButtonFunction));
            
            Vector2 TextSize = Font.MeasureString(Title);
            if (TextSize.X > Width)
            {
                Width = (int) TextSize.X + 10;
            }

            if (TextSize.Y > Height)
            {
                Height = (int) TextSize.Y + 6;
            }
            BoundingBottomRight.Y = Buttons.Count * Height + (Buttons.Count - 1) * Gap;
            BoundingBottomRight.X = Width;
            BoundingBottomRight += BoundingTopLeft;

            ButtonSize = new Vector2(Width, Height);
        }

        public void Update()
        {           
            MouseState mouse = Mouse.GetState();
            var MousePosition = new Vector2(mouse.X, mouse.Y);
            HighlightedId = -1;
            if (!IsInside(MousePosition, BoundingTopLeft, BoundingBottomRight))
            {
                return;
            }

            var CurrentPosition = BoundingTopLeft;
            for(int i = 0; i < Buttons.Count; i++, CurrentPosition.Y += (Gap + Height))
            {
                if (IsInside(MousePosition, CurrentPosition))
                {
                    if (mouse.LeftButton == ButtonState.Pressed)
                    {
                        if (OldMouse.LeftButton != mouse.LeftButton)
                        {
                           Buttons[i].Click();
                        }
                    }
                    else
                    {
                        HighlightedId = i;
                    }
                }
            }
            OldMouse = mouse;
        }

        public void Render(SpriteBatch Sprites)
        {
            var CurrentPosition = BoundingTopLeft;
            for (int i = 0; i < Buttons.Count; i++)
            {
                var color = HighlightedId == i ? HighlightedColor : ButtonColor;
                var border = Color.Black;

                Sprites.DrawString(Font, Buttons[i].Title, CurrentPosition + Vector2.UnitY, border);
                Sprites.DrawString(Font, Buttons[i].Title, CurrentPosition + Vector2.UnitX, border);
                Sprites.DrawString(Font, Buttons[i].Title, CurrentPosition - Vector2.UnitY, border);
                Sprites.DrawString(Font, Buttons[i].Title, CurrentPosition - Vector2.UnitX, border);

                Sprites.DrawString(Font, Buttons[i].Title, CurrentPosition, color);
                CurrentPosition.Y += Gap + Height;
            }
        }

        private Boolean IsInside(Vector2 Position, Vector2 TopLeft)
        {
            return IsInside(Position, TopLeft, TopLeft + ButtonSize);
        }

        private static Boolean IsInside(Vector2 Position, Vector2 TopLeft, Vector2 BottomRight)
        {            
            return (TopLeft.X < Position.X && Position.X < BottomRight.X &&
                TopLeft.Y < Position.Y && Position.Y < BottomRight.Y);
        }
    }
}
