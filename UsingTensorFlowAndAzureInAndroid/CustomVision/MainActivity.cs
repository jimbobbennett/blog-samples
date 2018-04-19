using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Plugin.Permissions;
using Plugin.TextToSpeech;

namespace CustomVision
{
	[Activity(Label = "@string/app_name", MainLauncher = true, Icon = "@mipmap/icon", Theme = "@style/MyTheme")]
    public class MainActivity : AppCompatActivity
    {
		public Android.Support.V7.Widget.Toolbar Toolbar { get; set; }

        Button takePhotoButton;
		private Button TakePhotoButton => takePhotoButton ?? (takePhotoButton = FindViewById<Button>(Resource.Id.take_photo_button));

        ImageView photoView;
		private ImageView PhotoView => photoView ?? (photoView = FindViewById<ImageView>(Resource.Id.photo));

        TextView resultLabel;
		private TextView ResultLabel => resultLabel ?? (resultLabel = FindViewById<TextView>(Resource.Id.result_label));

		ProgressBar progressBar;
		private ProgressBar ProgressBar => progressBar ?? (progressBar = FindViewById<ProgressBar>(Resource.Id.progressbar));

        readonly ImageClassifier imageClassifier = new ImageClassifier();

        protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			SetContentView(Resource.Layout.Main);

			Toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
			if (Toolbar != null)
				SetSupportActionBar(Toolbar);

			SupportActionBar.SetDisplayHomeAsUpEnabled(false);
			SupportActionBar.SetHomeButtonEnabled(false);

			TakePhotoButton.Click += TakePhotoButton_Click;
		}

		async void TakePhotoButton_Click(object sender, EventArgs e)
		{
			TakePhotoButton.Enabled = false;
            ProgressBar.Visibility = ViewStates.Visible;

            try
            {
                var image = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions { PhotoSize = PhotoSize.Medium });
                var bitmap = await BitmapFactory.DecodeStreamAsync(image.GetStreamWithImageRotatedForExternalStorage());

                PhotoView.SetImageBitmap(bitmap);
                var result = await Task.Run(() => imageClassifier.RecognizeImage(bitmap));
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                CrossTextToSpeech.Current.Speak($"I think it is {result}");
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                ResultLabel.Text = result;
            }
            finally
            {
                TakePhotoButton.Enabled = true;
                ProgressBar.Visibility = ViewStates.Invisible;
            }
		}

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults) => PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
    }
}

