using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UT4UU.Installer.GUI.Views
{
	public class ListBoxScrollToEnd : ListBox, IStyleable
	{
		Type IStyleable.StyleKey => typeof(ListBox);

		protected override void ItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			base.ItemsCollectionChanged(sender, e);

			if (Scroll != null)
			{
				if (Scroll is ScrollViewer scroll)
				{
					scroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
					scroll.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
				}

				double left = Scroll.Viewport.Height + Scroll.Offset.Y;
				double right = Scroll.Extent.Height;
				if (right - left <= 2)
				{
					Scroll.Offset = new Vector(0, Scroll.Extent.Height - Scroll.Viewport.Height + 1);
				}
			}
		}
	}
}
