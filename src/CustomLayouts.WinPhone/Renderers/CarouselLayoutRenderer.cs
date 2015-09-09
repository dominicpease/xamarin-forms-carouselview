using Xamarin.Forms;
using System.Reflection;
using System.Timers;
using System.ComponentModel;
using CustomLayouts.Controls;
using CustomLayouts.WinPhone.Renderers;
using Xamarin.Forms.Platform.WinPhone;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Animation;
using System;

[assembly: ExportRenderer(typeof(CarouselLayout), typeof(CarouselLayoutRenderer))]

namespace CustomLayouts.WinPhone.Renderers
{
	public class CarouselLayoutRenderer : ScrollViewRenderer
	{
		int _prevScrollX;
		int _deltaX;
		bool _motionDown;
		Timer _deltaXResetTimer;
		Timer _scrollStopTimer;
		ScrollViewerOffsetMediator mediator;
		bool animating = false;

		protected override void OnElementChanged(ElementChangedEventArgs<ScrollView> e)
		{
			mediator = new ScrollViewerOffsetMediator();

			base.OnElementChanged(e);
			if (e.NewElement == null) return;

			_deltaXResetTimer = new Timer(100) { AutoReset = false };
			_deltaXResetTimer.Elapsed += (object sender, ElapsedEventArgs args) =>
			{
				_deltaXResetTimer.Stop();
				ScrollToSelectedIndex(200);
			};

			e.NewElement.PropertyChanged += ElementPropertyChanged;
		}

		void ScrollToSelectedIndex(int  animationMS)
		{
			if (mediator.ScrollViewer != this.Control)
			{
				mediator.HorizontalOffset = this.Control.HorizontalOffset;
				mediator.ScrollViewer = this.Control;
			}

			animating = true;
			mediator.HorizontalOffset = this.Control.HorizontalOffset;

			Storyboard sb = new Storyboard();
			DoubleAnimation danim = new DoubleAnimation()
			{
				To = ((CarouselLayout)Element).SelectedIndex * Control.ActualWidth,
				FillBehavior = FillBehavior.HoldEnd,
				Duration = new Duration(TimeSpan.FromMilliseconds(animationMS)),
				EasingFunction = new QuadraticEase() { EasingMode = EasingMode.EaseOut }
			};
			Storyboard.SetTarget(danim, mediator);
			Storyboard.SetTargetProperty(danim, new PropertyPath("HorizontalOffset"));

			sb.Children.Add(danim);

			sb.Completed += (object s1, EventArgs e1) =>
			{
				animating = false;
			};
			sb.Begin();
		}

		void ElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Renderer")
			{
				int v = 0;
				RegisterForNotification(nameof(this.Control.HorizontalOffset), this.Control, new PropertyChangedCallback((s, args) => 
				{
					_deltaXResetTimer.Stop();
					UpdateSelectedIndex();
					_deltaXResetTimer.Start ();
				}));
			}
			if (e.PropertyName == CarouselLayout.SelectedIndexProperty.PropertyName && !_motionDown)
			{
				//ScrollToIndex(((CarouselLayout)this.Element).SelectedIndex);
				ScrollToSelectedIndex(500);
			}
		}

		void UpdateSelectedIndex()
		{
			if (animating || this.Control.ActualWidth == 0)
				return;

			var center = this.Control.HorizontalOffset + (this.Control.ActualWidth / 2);
			((CarouselLayout)Element).SelectedIndex = ((int)center) / ((int)this.Control.ActualWidth);
		}

		/// Listen for change of the dependency property 
		public void RegisterForNotification(string propertyName, FrameworkElement element, PropertyChangedCallback callback)
		{
			//Bind to a depedency property 
			System.Windows.Data.Binding b = new System.Windows.Data.Binding(propertyName) { Source = element };
			var prop = System.Windows.DependencyProperty.RegisterAttached("ListenAttached" + propertyName, typeof(object), typeof(UserControl), new System.Windows.PropertyMetadata(callback));
			element.SetBinding(prop, b);
		}
		

		//void HScrollViewTouch(object sender, TouchEventArgs e)
		//{
		//	e.Handled = false;

		//	switch (e.Event.Action)
		//	{
		//		case MotionEventActions.Move:
		//			_deltaXResetTimer.Stop();
		//			_deltaX = _scrollView.ScrollX - _prevScrollX;
		//			_prevScrollX = _scrollView.ScrollX;

		//			UpdateSelectedIndex();

		//			_deltaXResetTimer.Start();
		//			break;
		//		case MotionEventActions.Down:
		//			_motionDown = true;
		//			_scrollStopTimer.Stop();
		//			break;
		//		case MotionEventActions.Up:
		//			_motionDown = false;
		//			SnapScroll();
		//			_scrollStopTimer.Start();
		//			break;
		//	}
		//}

		//void UpdateSelectedIndex()
		//{
		//	var center = _scrollView.ScrollX + (_scrollView.Width / 2);
		//	var carouselLayout = (CarouselLayout)this.Element;
		//	carouselLayout.SelectedIndex = (center / _scrollView.Width);
		//}

		//void SnapScroll()
		//{
		//	var roughIndex = (float)_scrollView.ScrollX / _scrollView.Width;

		//	var targetIndex =
		//		_deltaX < 0 ? Math.Floor(roughIndex)
		//		: _deltaX > 0 ? Math.Ceil(roughIndex)
		//		: Math.Round(roughIndex);

		//	ScrollToIndex((int)targetIndex);
		//}

		//void ScrollToIndex(int targetIndex)
		//{
		//	var targetX = targetIndex * _scrollView.Width;
		//	_scrollView.Post(new Runnable(() => {
		//		_scrollView.SmoothScrollTo(targetX, 0);
		//	}));
		//}

		bool _initialized = false;
	}

	/// <summary>
	/// Mediator that forwards Offset property changes on to a ScrollViewer
	/// instance to enable the animation of Horizontal/VerticalOffset.
	/// </summary>
	public class ScrollViewerOffsetMediator : FrameworkElement
	{
		/// <summary>
		/// ScrollViewer instance to forward Offset changes on to.
		/// </summary>
		public ScrollViewer ScrollViewer
		{
			get { return (ScrollViewer)GetValue(ScrollViewerProperty); }
			set { SetValue(ScrollViewerProperty, value); }
		}
		public static readonly DependencyProperty ScrollViewerProperty =
			DependencyProperty.Register(
				"ScrollViewer",
				typeof(ScrollViewer),
				typeof(ScrollViewerOffsetMediator),
				new PropertyMetadata(OnScrollViewerChanged));
		private static void OnScrollViewerChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
		{
			var mediator = (ScrollViewerOffsetMediator)o;
			var scrollViewer = (ScrollViewer)(e.NewValue);
			if (null != scrollViewer)
			{
				scrollViewer.ScrollToHorizontalOffset(mediator.HorizontalOffset);
			}
		}

		/// <summary>
		/// HorizontalOffset property to forward to the ScrollViewer.
		/// </summary>
		public double HorizontalOffset
		{
			get { return (double)GetValue(HorizontalOffsetProperty); }
			set { SetValue(HorizontalOffsetProperty, value); }
		}
		public static readonly DependencyProperty HorizontalOffsetProperty =
			DependencyProperty.Register(
				"HorizontalOffset",
				typeof(double),
				typeof(ScrollViewerOffsetMediator),
				new PropertyMetadata(OnHorizontalOffsetChanged));

		public static void OnHorizontalOffsetChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
		{
			var mediator = (ScrollViewerOffsetMediator)o;
			if (null != mediator.ScrollViewer)
			{
				mediator.ScrollViewer.ScrollToHorizontalOffset((double)(e.NewValue));
			}
		}

		/// <summary>
		/// Multiplier for ScrollableHeight property to forward to the ScrollViewer.
		/// </summary>
		/// <remarks>
		/// 0.0 means "scrolled to top"; 1.0 means "scrolled to bottom".
		/// </remarks>
		public double ScrollableWidthMultiplier
		{
			get { return (double)GetValue(ScrollableWidthMultiplierProperty); }
			set { SetValue(ScrollableWidthMultiplierProperty, value); }
		}
		public static readonly DependencyProperty ScrollableWidthMultiplierProperty =
			DependencyProperty.Register(
				"ScrollableWidthMultiplier",
				typeof(double),
				typeof(ScrollViewerOffsetMediator),
				new PropertyMetadata(OnScrollableWidthMultiplierChanged));
		public static void OnScrollableWidthMultiplierChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
		{
			var mediator = (ScrollViewerOffsetMediator)o;
			var scrollViewer = mediator.ScrollViewer;
			if (null != scrollViewer)
			{
				scrollViewer.ScrollToHorizontalOffset((double)(e.NewValue) * scrollViewer.ScrollableWidth);
			}
		}
	}

}