using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Geolocation;
using System.Device.Location;
using System.Threading.Tasks;
using System.Net;
using Windows.UI.Popups;
using System.Text;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace NearMe
{
    /// <summary>
    /// This page show the list of the places that is near me
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        #region Private variables

        Geoposition myGeoposition;
        String fsClient = "H4QBPHTR22BCKABBMXEHHK4CJD51KHSWRGNLNL1ZCA4FCHRF";
        String fssecret = "NPMA2CJLXXE3GMC0YZCSVFNCZRFOUICZ1RWGPT2KDFUWQ0DH";
        int offset, limit;
        string query;
        bool full;
        double radius;
        bool busy;
        ObservableCollection<Venue> venues;
        List<Category> icategories;
        string[] strCategories = new string[] {"All", "Airport", "ATM",
                                                    "Bar", "Beer", "Brunch",
                                                    "Coffee", "Club",
                                                    "Drinks",
                                                    "Food", "Fun",
                                                    "Gym",
                                                    "Hotel",
                                                    "Karaoke",
                                                    "Library",
                                                    "Market",
                                                    "Nail", "Nightlife",
                                                    "Pizza", "Park",
                                                    "Restaurant", "Resort",
                                                    "Shopping", "Spa", "School",
                                                    "University",
                                                    "Yoga",
                                                    "Zoo"};

        #endregion

        #region Public variable
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            offset = 0;
            limit = 20;
            query = "All";
            Radius = 5000;
            venues = new ObservableCollection<Venue>();
            lstBox.ItemsSource = Venues;
            full = false;
            InitICategories();
            HideMenuCategories();
            DataContext = this;
        }

        #region Properties
        public ObservableCollection<Venue> Venues { get { return venues; } }
        public List<Category> Categories 
        {
            get
            {
                return icategories;
            }
            set
            {
                icategories = value;
                NotifyPropertyChanged("ICategories");
            }
        }
        public string Query
        {
            get
            {
                return query;
            }
            set
            {
                query = value;
                NotifyPropertyChanged("Query");
            }
        }
        public double Radius
        {
            get
            {
                return radius;
            }
            set
            {
                radius = value;
                NotifyPropertyChanged("Radius");
            }
        }
        #endregion

        #region Methods

        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(info));
        }

        /// <summary>
        /// init a list of categories
        /// </summary>
        private void InitICategories()
        {
            icategories = new List<Category>();
            foreach (string cate in strCategories)
                icategories.Add(new Category() { name = cate });
        }

        /// <summary>
        /// get my positon
        /// </summary>
        /// <returns></returns>
        private async Task<Geoposition> getCoordinates()
        {
            Geolocator geolocator = new Geolocator();
            geolocator.DesiredAccuracyInMeters = 50;

            try
            {
                return await geolocator.GetGeopositionAsync(maximumAge: TimeSpan.FromMinutes(5), timeout: TimeSpan.FromSeconds(10));
            }
            catch (Exception)
            {
                MessageDialog dialog = new MessageDialog("Không thể xác định vị trí của bạn", "Thông báo");
                return null;
            }
        }

        /// <summary>
        /// get place that is near by me
        /// </summary>
        /// <returns></returns>
        private async Task<bool> getNearbyPlaces()
        {
            try
            {
                String datatopost = "?client_id=" + fsClient + "&client_secret=" + fssecret + "&v=20130815&ll="
                                    + myGeoposition.Coordinate.Latitude
                                    + "," + myGeoposition.Coordinate.Longitude
                                    + "&limit=" + limit + "&offset=" + offset;
                if (query != "All")
                    datatopost += "&query=" + query.Trim(new char[] { ' ' });
                if (Radius != 0.0)
                    datatopost += "&radius=" + Radius.ToString();
                Uri address = new Uri("https://api.foursquare.com/v2/venues/explore" + datatopost);
                // Create the web request
                HttpWebRequest request = WebRequest.Create(address) as HttpWebRequest;
                // Get response
                using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        //To obtain response body
                        using (Stream streamResponse = response.GetResponseStream())
                        {
                            using (StreamReader streamRead = new StreamReader(streamResponse, Encoding.UTF8))
                            {
                                JSON nearbyVenues = JsonConvert.DeserializeObject<JSON>(streamRead.ReadToEnd().ToString());
                                int oldCount = venues.Count;
                                foreach (Group group in nearbyVenues.response.groups)
                                    foreach (Item item in group.items)
                                    {
                                        venues.Add(item.venue.Clone());
                                        ++offset;
                                    }
                                if (venues.Count - oldCount < limit)
                                    full = true;
                            }
                        }
                    }
                }
                return true;
            }
            catch (Exception)
            {
                MessageDialog dialog = new MessageDialog("Đã có lỗi xảy ra, vui lòng thử lại sau", "Thông báo");
                dialog.ShowAsync();
                return false;
            }
        }


        void ShowMenuCategories()
        {
            gridBlur.Visibility = Windows.UI.Xaml.Visibility.Visible;
            gridMenuCategory.Visibility = Windows.UI.Xaml.Visibility.Visible;
            txtRadius.Text = Radius.ToString();
            lstBoxCategories.ScrollIntoView(lstBoxCategories.Items[0]);
            if (lstBoxCategories.SelectedIndex < 0)
                lstBoxCategories.SelectedIndex = 0;
        }

        void HideMenuCategories()
        {
            gridBlur.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            gridMenuCategory.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private async Task<bool> Refresh()
        {
            venues.Clear();
            full = false;
            offset = 0;
            Query = (lstBoxCategories.SelectedItem as Category).name;
            Radius = double.Parse(txtRadius.Text.ToString());
            gridLoading.Visibility = Windows.UI.Xaml.Visibility.Visible;
            busy = true;
            statusText.Text = "Đang tải...";
            bool result = await getNearbyPlaces();
            if (result)
                gridLoading.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            else statusText.Text = "Đã xảy ra lỗi. Chạm để tải lại.";
            busy = false;
            return result;
        }

        #endregion

        #region Events

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
                return;
            gridLoading.Visibility = Windows.UI.Xaml.Visibility.Visible;
            busy = true;
            statusText.Text = "Đang xác định vị trí của bạn...";
            myGeoposition = await getCoordinates();
            if (myGeoposition == null)
            {
                statusText.Text = "Đã xảy ra lỗi. Chạm để tải lại.";
                busy = false;
                return;
            }
            statusText.Text = "Đang tải...";
            bool result = await getNearbyPlaces();
            if (result)
                gridLoading.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            else statusText.Text = "Đã xảy ra lỗi. Chạm để tải lại.";
            busy = false;
        }

        private void lstBox_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (lstBox.SelectedIndex == -1)
                return;
            string id = Venues[lstBox.SelectedIndex].id;
            lstBox.SelectedIndex = -1;
            Frame.Navigate(typeof(VenueDetailPage), id);
        }

        private async void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (!e.IsIntermediate)
            {
                if (scrViewr.VerticalOffset >= lstBox.ActualHeight - scrViewr.ActualHeight && !full)
                {
                    txtLoading.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    await getNearbyPlaces();
                    txtLoading.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                }
            }
            if (scrViewr.VerticalOffset == 0.0)
                gotoTop.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            else gotoTop.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        private void Ellipse_Tapped(object sender, TappedRoutedEventArgs e)
        {
            gotoTop.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            scrViewr.ChangeView(0.0, 0.0, null);
        }

        private void scrViewr_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            if (scrViewr.VerticalOffset == 0.0)
                gotoTop.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            else gotoTop.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        private void Image_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (gridBlur.Visibility == Windows.UI.Xaml.Visibility.Visible)
                HideMenuCategories();
            else ShowMenuCategories();
        }

        private void gridBlur_Tapped(object sender, TappedRoutedEventArgs e)
        {
            HideMenuCategories();
        }

        private async void gridLoading_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (busy)
                return;
            busy = true;
            statusText.Text = "Đang xác định vị trí của bạn...";
            myGeoposition = await getCoordinates();
            if (myGeoposition == null)
            {
                statusText.Text = "Đã xảy ra lỗi. Chạm để tải lại.";
                busy = false;
                return;
            }
            statusText.Text = "Đang tải...";
            bool result = await getNearbyPlaces();
            if (result)
                gridLoading.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            else statusText.Text = "Đã xảy ra lỗi. Chạm để tải lại.";
            busy = false;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            HideMenuCategories();
            await Refresh();
        }
        #endregion

        #region Public classes

        public class Meta
        {
            public int code { get; set; }
        }

        public class Location
        {
            public int distance { get; set; }
        }

        public class Icon
        {
            public string prefix { get; set; }
            public string suffix { get; set; }

            public string url { get { return prefix + "bg_64" + suffix; } }
        }

        public class Category
        {
            public string name { get; set; }
            public Icon icon { get; set; }
        }

        public class Venue
        {
            public string id { get; set; }
            public string name { get; set; }
            public Location location { get; set; }
            public List<Category> categories { get; set; }

            public Venue Clone()
            {
                Venue venue = new Venue();
                venue.id = this.id;
                venue.name = this.name;
                venue.location = this.location;
                venue.categories = this.categories;
                return venue;
            }
        }

        public class Item
        {
            public Venue venue { get; set; }
        }

        public class Group
        {
            public List<Item> items { get; set; }
        }

        public class Response
        {
            public List<Group> groups { get; set; }
        }

        public class JSON
        {
            public Meta meta { get; set; }
            public Response response { get; set; }
        }

        #endregion
    }
}