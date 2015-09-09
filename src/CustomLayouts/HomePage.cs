using System;
using Xamarin.Forms;
using CustomLayouts.Controls;
using CustomLayouts.ViewModels;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

namespace CustomLayouts
{
	public class HomePage : ContentPage
	{
		View _tabs;

		RelativeLayout relativeLayout;

		CarouselLayout.IndicatorStyleEnum _indicatorStyle;

		SwitcherPageViewModel viewModel;
		CarouselLayout pagesCarousel;

		public HomePage(CarouselLayout.IndicatorStyleEnum indicatorStyle)
		{
			_indicatorStyle = indicatorStyle;

			viewModel = new SwitcherPageViewModel();
			BindingContext = viewModel;

			Title = _indicatorStyle.ToString();

			relativeLayout = new RelativeLayout 
			{
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand
			};

			pagesCarousel = CreatePagesCarousel();
			var dots = CreatePagerIndicatorContainer();
			_tabs = CreateTabs();

			switch(pagesCarousel.IndicatorStyle)
			{
				case CarouselLayout.IndicatorStyleEnum.Dots:
					relativeLayout.Children.Add (pagesCarousel,
						Constraint.RelativeToParent ((parent) => { return parent.X; }),
						Constraint.RelativeToParent ((parent) => { return parent.Y; }),
						Constraint.RelativeToParent ((parent) => { return parent.Width; }),
						Constraint.RelativeToParent ((parent) => { return parent.Height; })
					);

					relativeLayout.Children.Add (dots, 
						Constraint.Constant (0),
						Constraint.RelativeToView (pagesCarousel, 
							(parent,sibling) => { return sibling.Height - 38; }),
						Constraint.RelativeToParent (parent => parent.Width),
						Constraint.Constant (38)
					);
					break;
				case CarouselLayout.IndicatorStyleEnum.Tabs:
					var tabsHeight = 50;
					relativeLayout.Children.Add (_tabs, 
						Constraint.Constant (0),
						Constraint.RelativeToParent ((parent) => { return parent.Height - tabsHeight; }),
						Constraint.RelativeToParent (parent => parent.Width),
						Constraint.Constant (tabsHeight)
					);

					relativeLayout.Children.Add (pagesCarousel,
						Constraint.RelativeToParent ((parent) => { return parent.X; }),
						Constraint.RelativeToParent ((parent) => { return parent.Y; }),
						Constraint.RelativeToParent ((parent) => { return parent.Width; }),
						Constraint.RelativeToView (_tabs, (parent, sibling) => { return parent.Height - (sibling.Height); })
					);
					break;
				default:
					relativeLayout.Children.Add (pagesCarousel,
						Constraint.RelativeToParent ((parent) => { return parent.X; }),
						Constraint.RelativeToParent ((parent) => { return parent.Y; }),
						Constraint.RelativeToParent ((parent) => { return parent.Width; }),
						Constraint.RelativeToParent ((parent) => { return parent.Height; })
					);
					break;
			}

			Content = relativeLayout;
		}

		CarouselLayout CreatePagesCarousel ()
		{
			var carousel = new CarouselLayout {
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand,
				IndicatorStyle = _indicatorStyle,
				ItemTemplate = new DataTemplate(typeof(HomeView))
			};
			carousel.SetBinding(CarouselLayout.ItemsSourceProperty, "Pages");
			carousel.SetBinding(CarouselLayout.SelectedItemProperty, "CurrentPage", BindingMode.TwoWay);

			return carousel;
		}

		View CreatePagerIndicatorContainer()
		{
			return new Grid {
				Children = { CreatePagerIndicators() }
			};
		}

		View CreatePagerIndicators()
		{
			var pagerIndicator = new PagerIndicatorDots() { DotSize = 15, DotColor = Color.Black, HeightRequest = 50 };
			pagerIndicator.SetBinding (PagerIndicatorDots.ItemsSourceProperty, "Pages");
			pagerIndicator.SetBinding (PagerIndicatorDots.SelectedItemProperty, "CurrentPage");
			return pagerIndicator;
		}

		View CreateTabsContainer()
		{
			return new StackLayout {
				Children = { CreateTabs() }
			};
		}

		View CreateTabs()
		{
			var pagerIndicator = new PagerIndicatorTabs() { HorizontalOptions = LayoutOptions.CenterAndExpand };
			pagerIndicator.RowDefinitions.Add(new RowDefinition() { Height = 50 });
#if WINDOWS_PHONE
			// For some reason, the binding converter in the #else doesn't seem to get invoked, no matter which overload of SetBinding is used. It is constructed,
			// but after it is passed to SetBinding the Convert() method is never called and an exception is thrown. So here's a replacement for the databinding
			// logic on Windows Phone.

			EventHandler action = (object sender, EventArgs e) =>
			{
				SwitcherPageViewModel vm = this.BindingContext as SwitcherPageViewModel;
				SpacingConverter sc = new SpacingConverter();
				pagerIndicator.ColumnDefinitions = (ColumnDefinitionCollection)sc.Convert(vm.Pages, typeof(ColumnDefinitionCollection), null, System.Globalization.CultureInfo.CurrentCulture);
            };

			this.BindingContextChanged += action;

			action(null, EventArgs.Empty);

			pagerIndicator.PropertyChanged += PagerIndicator_PropertyChanged;
#else
			pagerIndicator.SetBinding(PagerIndicatorTabs.ColumnDefinitionsProperty, new Binding("Pages", BindingMode.Default, new SpacingConverter(), null));
#endif
			pagerIndicator.SetBinding (PagerIndicatorTabs.ItemsSourceProperty, "Pages");
			pagerIndicator.SetBinding (PagerIndicatorTabs.SelectedItemProperty, "CurrentPage");

			return pagerIndicator;
		}

		bool setting = false;
		private void PagerIndicator_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			PagerIndicatorTabs pagerIndicator = sender as PagerIndicatorTabs;
			if (setting || e.PropertyName != nameof(pagerIndicator.SelectedItem) || !(pagerIndicator.SelectedItem is HomeViewModel))
				return;

			setting = true;
			pagesCarousel.SelectedItem = ((HomeViewModel)pagerIndicator.SelectedItem);
			setting = false;
		}
	}

	public class SpacingConverter : IValueConverter
	{
		public SpacingConverter()
		{
		}

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			Debug.WriteLine("Entered SpacingConverter.Convert");
			var items = value as IEnumerable<HomeViewModel>;

			var collection = new ColumnDefinitionCollection();

			if (items == null)
				return collection;

			foreach(var item in items)
			{
				collection.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
			}
			return collection;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}

