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
        private Android.Support.V7.Widget.Toolbar Toolbar { get; set; }

        Button _takePhotoButton;
		private Button TakePhotoButton => _takePhotoButton ??= FindViewById<Button>(Resource.Id.take_photo_button);

        Button _selectPhotoButton;
        private Button SelectPhotoButton => _selectPhotoButton ??= FindViewById<Button>(Resource.Id.select_photo_button);


        ImageView _photoView;
		private ImageView PhotoView => _photoView ??= FindViewById<ImageView>(Resource.Id.photo);

        TextView _resultLabel;
		private TextView ResultLabel => _resultLabel ??= FindViewById<TextView>(Resource.Id.result_label);

		ProgressBar _progressBar;
		private ProgressBar ProgressBar => _progressBar ??= FindViewById<ProgressBar>(Resource.Id.progressbar);

        readonly ImageClassifier _imageClassifier = new ImageClassifier();


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
            SelectPhotoButton.Click += SelectPhotoButton_Click;
		}

        private async void SelectPhotoButton_Click(object sender, EventArgs e)
        {
            SelectPhotoButton.Enabled = false;
            TakePhotoButton.Enabled = false;
            ProgressBar.Visibility = ViewStates.Visible;

            try
            {
                var image = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions{PhotoSize = PhotoSize.Medium});
                ImageProcessing(image);
            }
            finally
            {
                SelectPhotoButton.Enabled = true;
                TakePhotoButton.Enabled = true;
                ProgressBar.Visibility = ViewStates.Invisible;
            }
        }

        async void TakePhotoButton_Click(object sender, EventArgs e)
		{
			TakePhotoButton.Enabled = false;
            ProgressBar.Visibility = ViewStates.Visible;

            try
            {
                var image = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions { PhotoSize = PhotoSize.Medium });
                ImageProcessing(image);
            }
            finally
            {
                TakePhotoButton.Enabled = true;
                SelectPhotoButton.Enabled = true;
                ProgressBar.Visibility = ViewStates.Invisible;
            }
		}

        private async void ImageProcessing(MediaFile image)
        {
            var bitmap = await BitmapFactory.DecodeStreamAsync(image.GetStreamWithImageRotatedForExternalStorage());
            PhotoView.SetImageBitmap(bitmap);
            var result = await Task.Run(() => _imageClassifier.RecognizeImage(bitmap));
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            CrossTextToSpeech.Current.Speak($"I think it is {result}");
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            ResultLabel.Text = result;
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults) => PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
    }
}

