using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Phone.UI.Input;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace NearMe
{
    /// <summary>
    /// This page show detail of venue
    /// </summary>
    public sealed partial class VenueDetailPage : Page, INotifyPropertyChanged
    {
        #region Private variables

        String fsClient = "H4QBPHTR22BCKABBMXEHHK4CJD51KHSWRGNLNL1ZCA4FCHRF";
        String fssecret = "NPMA2CJLXXE3GMC0YZCSVFNCZRFOUICZ1RWGPT2KDFUWQ0DH";
        Venue ivenue;

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public VenueDetailPage()
        {
            this.InitializeComponent();
            DataContext = this;
            HardwareButtons.BackPressed += HardwareButtons_BackPressed;
        }

        #region Public properties

        public Venue iVenue
        {
            get
            {
                return ivenue;
            }
            set
            {
                ivenue = value;
                NotifyPropertyChanged("iVenue");
            }
        }

        #endregion

        #region Public variable

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Events

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            string id = e.Parameter as string;
            gridLoading.Visibility = Windows.UI.Xaml.Visibility.Visible;
            bool result = await getVenueDetail(id);
            if (!result) txtStatus.Text = "Error";
            else
            {
                gridLoading.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                if (iVenue.likes.count <= 1)
                    txtLike.Text = " like";
                if (iVenue.stats.checkinsCount <= 1)
                    txtCheckin.Text = " checkin";
                MapIcon mapIcon = new MapIcon();
                mapIcon.Image = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/pushpin.png"));
                mapIcon.Title = iVenue.name;
                mapIcon.Location = new Geopoint(new BasicGeoposition()
                {
                    Latitude = iVenue.location.lat,
                    Longitude = iVenue.location.lng
                });
                mapIcon.NormalizedAnchorPoint = new Point(0.5, 0.5);
                map.MapElements.Add(mapIcon);
                await map.TrySetViewAsync(mapIcon.Location, 15D, 0, 0, MapAnimationKind.Linear);
            }
        }

        void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            Frame thisFrame = Window.Current.Content as Frame;
            if(thisFrame != null && thisFrame.CanGoBack)
            {
                thisFrame.GoBack();
                e.Handled = true;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// get detail of venue
        /// </summary>
        /// <param name="venueid"> id of venue </param>
        /// <returns></returns>
        private async Task<bool> getVenueDetail(string venueid)
        {
            try
            {
                String datatopost = "?client_id=" + fsClient + "&client_secret=" + fssecret + "&v=20130815";
                Uri address = new Uri("https://api.foursquare.com/v2/venues/" + venueid + datatopost);
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
                                JSON json = JsonConvert.DeserializeObject<JSON>(streamRead.ReadToEnd().ToString());
                                iVenue = json.response.venue;
                            }
                        }
                    }
                }
                return true;
            }
            catch (Exception)
            {
                MessageDialog dialog = new MessageDialog("Some error occured", "Message");
                dialog.ShowAsync();
                return false;
            }
        }

        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(info));
        }

        #endregion

        #region Public classes

        public class Contact
        {
            public string formattedPhone { get; set; }
        }

        public class Location
        {
            public double lat { get; set; }
            public double lng { get; set; }
            public List<string> formattedAddress;
            public string address
            {
                get
                {
                    string result = "● ";
                    int i;
                    for (i = 0; i < formattedAddress.Count - 1; ++i)
                        result += formattedAddress[i] + ", ";
                    result += formattedAddress[i];
                    return result;
                }
            }
        }

        public class Icon
        {
            public string prefix { get; set; }
            public string suffix { get; set; }

            public string url { get { return prefix + suffix; } }
        }

        public class Category
        {
            public string name { get; set; }
            public Icon icon { get; set; }
        }

        public class Stat
        {
            public int checkinsCount { get; set; }
        }

        public class Like
        {
            public int count { get; set; }
        }

        public class Venue
        {
            public string id { get; set; }
            public string name { get; set; }
            public Contact contact { get; set; }
            public Location location { get; set; }
            public List<Category> categories { get; set; }
            public string category
            {
                get
                {
                    string result = "● ";
                    int i;
                    for (i = 0; i < categories.Count - 1; ++i)
                        result += categories[i].name + ", ";
                    result += categories[i].name;
                    return result;
                }
            }
            public Stat stats { get; set; }
            public string url { get; set; }
            public Like likes { get; set; }
        }

        public class Response
        {
            public Venue venue { get; set; }
        }

        public class JSON
        {
            public Response response { get; set; }
        }

        #endregion
    }
}