using System;

using Android.Content;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Android.Icu.Number;

namespace uk.andyjohnson.HardView2
{
    public class GestureDecoder : Java.Lang.Object, GestureDetector.IOnGestureListener, ScaleGestureDetector.IOnScaleGestureListener
    {
        public GestureDecoder(Context context, Window window, ImageView imageView)
        {
            this.context = context;
            this.window = window;
            this.imageView = imageView;
            this.touchGestureDetector = new GestureDetector(context, this as GestureDetector.IOnGestureListener);
            this.scaleGestureDetector = new ScaleGestureDetector(context, this as ScaleGestureDetector.IOnScaleGestureListener);

            this.initialPosition = new Point((int)imageView.GetX(), (int)imageView.GetY());
        }


        private readonly Context context;
        private readonly Window window;
        private readonly ImageView imageView;
        private readonly GestureDetector touchGestureDetector;
        private readonly ScaleGestureDetector scaleGestureDetector;
        private readonly Point initialPosition;

        private bool inScale = false;
        private const int swipeThresholdVelocity = 500;



        public bool OnTouchEvent(MotionEvent e)
        {
            return touchGestureDetector.OnTouchEvent(e) || scaleGestureDetector.OnTouchEvent(e);
        }



        public enum Gesture
        { 
            None,
            Next,
            Previous,
            First,
            Last,
            Random,
            Reset,
            ShowMenuBar
        }

        public event EventHandler<Gesture> Gestured;




        private float scaleFactor = 1.0f;

        public float MaxScaleFactor { get; set; } = 5.0f;
        public float MinScaleFactor { get; set; } = 1.0f;
        public float ScaleEpsilon { get; set; } = 0.01f;

        public void Reset()
        {
            this.scaleFactor = 1.0f;
            imageView.SetX(initialPosition.X);
            imageView.SetY(initialPosition.Y);
            imageView.ScaleX = 1.0f;
            imageView.ScaleY = 1.0f;
        }



        #region Gesture Implementation

        private enum HorzRgn
        {
            Left,
            Middle,
            Right
        }

        private enum VertRgn
        {
            Top,
            Middle,
            Bottom
        }

        private enum Press
        {
            Short,
            Long
        }


        private void DecodeGesture((HorzRgn hr, VertRgn vr, Press pr) gestureParts)
        {
            var gesture = Gesture.None;

            switch (gestureParts.pr)
            {
                case Press.Short:
                    switch (gestureParts)
                    {
                        case var t when t.vr == VertRgn.Top:
                            gesture = Gesture.ShowMenuBar;
                            break;
                        case var t when t.hr == HorzRgn.Left && (t.vr == VertRgn.Middle || t.vr == VertRgn.Bottom):
                            gesture = Gesture.Previous;
                            break;                        case var t when t.hr == HorzRgn.Right && (t.vr == VertRgn.Middle || t.vr == VertRgn.Bottom):
                            gesture = Gesture.Next;
                            break;
                        case var t when t.hr == HorzRgn.Middle && t.vr == VertRgn.Bottom:
                            gesture = Gesture.Random;
                            break;
                    }
                    break;
                case Press.Long:
                    switch (gestureParts)
                    {
                        case var t when t.hr == HorzRgn.Left && (t.vr == VertRgn.Middle || t.vr == VertRgn.Bottom):
                            gesture = Gesture.First;
                            break;
                        case var t when t.hr == HorzRgn.Right && (t.vr == VertRgn.Middle || t.vr == VertRgn.Bottom):
                            gesture = Gesture.Last;
                            break;
                        case var t when t.hr == HorzRgn.Middle && t.vr == VertRgn.Middle:
                            gesture = Gesture.Reset;
                            break;
                    };
                    break;
            }

            if (gesture != Gesture.None)
            {
                OnGestured(gesture);
            }
        }


        private void OnGestured(Gesture gesture)
        {
            var ev = this.Gestured;
            if (ev != null)
            {
                ev.Invoke(this, gesture);
            }
        }


        private (HorzRgn horizontalRegion, VertRgn verticalRegion) GetScreenRegion(
            MotionEvent e)
        {
            var w = (float)window.DecorView.Width;
            var h = (float)window.DecorView.Height;

            HorzRgn hr;
            if (e.GetX() < w * 0.25f)
                hr = HorzRgn.Left;
            else if (e.GetX() > w * 0.75f)
                hr = HorzRgn.Right;
            else
                hr = HorzRgn.Middle;

            VertRgn vr;
            if (e.GetY() < h * 0.25f)
                vr = VertRgn.Top;
            else if (e.GetY() > h * 0.75f)
                vr = VertRgn.Bottom;
            else
                vr = VertRgn.Middle;

            return (hr, vr);
        }

        #region GestureDetector.IOnGestureListener

        // Methods here have to be public to implement IOnGestureListener

        public bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
        {
            bool result = false;
            float dY = e2.GetY() - e1.GetY();
            float dX = e2.GetX() - e1.GetX();
            if (Math.Abs(dX) > Math.Abs(dY))
            {
                if ((Math.Abs(dX) > swipeThresholdVelocity) && (Math.Abs(velocityX) > swipeThresholdVelocity))
                {
                    if (dX > 0)
                    {
                        DecodeGesture((HorzRgn.Left, VertRgn.Middle, Press.Short));  // Previous
                    }
                    else
                    {
                        DecodeGesture((HorzRgn.Right, VertRgn.Middle, Press.Short));  // Next
                    }
                    result = true;
                }
            }
            else if ((Math.Abs(dY) > swipeThresholdVelocity) && (Math.Abs(velocityY) > swipeThresholdVelocity))
            {
                if (dY > 0)
                {
                    // OnSwipeDown();
                }
                else
                {
                    // OnSwipeUp();
                }
                result = true;
            }

            return result;
        }


        public void OnLongPress(MotionEvent e)
        {
            var sr = GetScreenRegion(e);
            DecodeGesture((sr.horizontalRegion, sr.verticalRegion, Press.Long));
        }


        public bool OnSingleTapUp(MotionEvent e)
        {
            var sr = GetScreenRegion(e);
            DecodeGesture((sr.horizontalRegion, sr.verticalRegion, Press.Short));
            return true;
        }


        public bool OnDown(MotionEvent e)
        {
            // Intentionally empty
            return false;
        }


        public bool OnScroll(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
        {
            if (inScale)
            {
                // Ignore any fake scroll events while scaling
                return true;
            }

            // Scale the distances for a more responsive scroll.
            const float scrollSf = 2.0f;
            distanceX *= scrollSf;
            distanceY *= scrollSf;

            // Calculate new X pos.
            float newX = imageView.GetX() - distanceX;
            if (newX > (imageView.Width * scaleFactor - imageView.Width) / 2)
            {
                newX = (imageView.Width* scaleFactor - imageView.Width) / 2;
            }
            else if (newX < -((imageView.Width * scaleFactor - imageView.Width) / 2))
            {
                newX = -((imageView.Width * scaleFactor - imageView.Width) / 2);
            }

            // Calculate new Y pos.
            float newY = imageView.GetY() - distanceY;
            if (newY > (imageView.Height * scaleFactor - imageView.Height) / 2)
            {
                newY = (imageView.Height * scaleFactor - imageView.Height) / 2;
            }
            else if (newY < -((imageView.Height * scaleFactor - imageView.Height) / 2))
            {
                newY = -((imageView.Height * scaleFactor - imageView.Height) / 2);
            }

            // Update the image view.
            imageView.SetX(newX);
            imageView.SetY(newY);

            return true;
        }


        public void OnShowPress(MotionEvent e)
        {
            // Intentionally empty
        }

        #endregion GestureDetector.IOnGestureListener

        #endregion Gesture Implementation



        #region Scaling Implementation

        #region ScaleGestureDetector.IOnScaleGestureListener

        public bool OnScale(ScaleGestureDetector detector)
        {

            // Calculate the new scale factor.
            // Note that detector.ScaleFactor is "the scaling factor from the _previous_ scale event to the _current_ event",
            // so we multiply the previous one by it.
            var newSf = this.scaleFactor * detector.ScaleFactor;
            newSf = Math.Max(Math.Min(newSf, this.MaxScaleFactor), this.MinScaleFactor);
            this.scaleFactor = ((float)((int)(newSf * 100))) / 100; // Change precision to help with jitter when user just rests their fingers

            this.imageView.ScaleX = this.scaleFactor;
            this.imageView.ScaleY = this.scaleFactor;

            return true;
        }

        public bool OnScaleBegin(ScaleGestureDetector detector)
        {
            inScale = true;
            return true;  // Continue to recognise the gesture
        }

        public void OnScaleEnd(ScaleGestureDetector detector)
        {
            inScale = false;
        }

        #endregion ScaleGestureDetector.IOnScaleGestureListener

        #endregion Scaling Implementation
    }
}