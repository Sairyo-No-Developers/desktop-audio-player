﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Xml.Schema;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using WebSocket4Net;
using Newtonsoft.Json;

namespace Desktop_Audio_Player
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MediaPlayer mediaPlayer = new MediaPlayer();
        string filename;
        bool slider_clicked = false;
        double totalTime = 1.0;
        bool is_total_time_set = false;
        bool play_not_pause = true;
        double volume = 1.0;
        bool is_muted = false;
        bool slider_to_be_updated = false;
        string song_title = "";
        string song_info = "";
        bool scroll_title = false;
        bool scroll_info = false;
        bool is_syncing = false;
        string session = "";
        bool update_sync = false;
        int updated_time = 0;
        string is_playing = "0";
        string last_is_playing = "0";
        WebSocket sync_socket;
        bool is_host = false;
        YoutubeClient youtube = new YoutubeClient();
        public MainWindow(bool open_with, string filename = "") {
            InitializeComponent();
            file_search.Visibility = Visibility.Visible;
            youtube_search.Visibility = Visibility.Visible;
            controls.Visibility = Visibility.Hidden;
            MouseDown += Window_MouseDown;
            slider_timer.AddHandler(MouseLeftButtonDownEvent,
                      new MouseButtonEventHandler(slider_pressed),
                      true);
            slider_timer.AddHandler(MouseLeftButtonUpEvent,
                      new MouseButtonEventHandler(slider_unpressed),
                      true);
            slider_timer.ValueChanged += slider_changed;
            slider_volume.AddHandler(MouseLeftButtonUpEvent,
                      new MouseButtonEventHandler(volume_changed),
                      true);
            slider_volume.ValueChanged += volume_changed;
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(650);
            timer.Tick += scroll_tick;
            timer.Start();
            if (open_with)
            {
                Open_With_Func(filename);
            }
           
        }

        private void scroll_tick(object sender, EventArgs e)
        {
            if (scroll_title)
            {
                song_title = song_title.Substring(1, song_title.Length - 1) + song_title.Substring(0, 1);
                song_title_xaml.Text = song_title.Substring(0, 22);
            }
            if (scroll_info)
            {
                song_info = song_info.Substring(1, song_info.Length - 1) + song_info.Substring(0, 1);
                song_info_xaml.Text = song_info.Substring(0, 32);
                Debug.WriteLine(song_info);
            }
        }

        private void slider_pressed(object sender, MouseButtonEventArgs e)
        {
            slider_clicked = true;
            int time_changed = (int)(((slider_timer.Value / 10) * totalTime));
            TimeSpan my_time = new TimeSpan(0, 0, 0, 0, time_changed);
            mediaPlayer.Position = my_time;
            curr_time.Text = my_time.ToString("hh':'mm':'ss");
            slider_to_be_updated = true;
        }


        private void slider_changed(object sender, RoutedEventArgs e)
        {
            if (slider_to_be_updated)
            {
                int time_changed = (int)(((slider_timer.Value / 10) * totalTime));
                TimeSpan my_time = new TimeSpan(0, 0, 0, 0, time_changed);
                mediaPlayer.Position = my_time;
                slider_to_be_updated = false;
            }            
        }

        private void play_pause(object sender, RoutedEventArgs e)
        {
            if (is_playing == "0")
            {
                BT_Click_Play(sender, e);
                play_not_pause = false;
            }
            else
            {
                BT_Click_Pause(sender, e);
                play_not_pause = true;
            }
        }

        private void slider_unpressed(object sender, MouseButtonEventArgs e)
        {
            Dictionary<string, string> data;
            string data_str = "";
            int time_changed = (int)(((slider_timer.Value / 10) * totalTime));
            TimeSpan my_time = new TimeSpan(0, 0, 0, 0, time_changed);
            mediaPlayer.Position = my_time;
            curr_time.Text = my_time.ToString("hh':'mm':'ss");
            slider_clicked = false;
            if (is_host)
            {
                data = new Dictionary<string, string>()
                        {
                            {"is_playing", is_playing },
                            {"to_be_updated", "1"},
                            {"position", time_changed.ToString()},
                        };
                data = new Dictionary<string, string>()
                        {
                            {"message", JsonConvert.SerializeObject(data)}
                        };
                data_str = JsonConvert.SerializeObject(data);
                sync_socket.Send(data_str);
            }
        }

        private void volume_changed(object sender, RoutedEventArgs e)
        {
            volume = slider_volume.Value / 10;
            mediaPlayer.Volume = volume;
            if (volume == 0.0)
            {
                is_muted = true;
                volume_icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.VolumeMute;
            }
            else
            {
                is_muted = false;
                volume_icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.VolumeHigh;
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void Open_With_Func(string filename)
        {
            File_Name.Text = filename;
            mediaPlayer.Open(new Uri(filename));
            controls.Visibility = Visibility.Visible;
            file_search.Visibility = Visibility.Hidden;
            youtube_search.Visibility = Visibility.Hidden;
            sync_bar.Visibility = Visibility.Collapsed;
            var tfile = TagLib.File.Create(filename);
            song_title = tfile.Tag.Title;
            song_info = tfile.Tag.JoinedPerformers + " - " + tfile.Tag.JoinedPerformers;
            if (song_title.Length > 22)
            {
                scroll_title = true;
                song_title = song_title + "    ";
                song_title_xaml.Text = song_title.Substring(0, 22);
            }
            else
            {
                scroll_title = false;
                song_title_xaml.Text = song_title;
            }
            if (song_info.Length > 32)
            {
                scroll_info = true;
                song_info = song_info + "    ";
                song_info_xaml.Text = song_info;
            }
            else
            {
                scroll_info = false;
                song_info_xaml.Text = song_info;
            }
            Open_With_Play();
        }
        private void BT_Click_Open(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = false;
            fileDialog.DefaultExt = ".mp3";
            fileDialog.Filter = "All Audio Files|*.mp3;*.flac;*.wav;*m4a|MP3 File (*.mp3)|*.mp3|FLAC File (*.flac)|*.flac|WAV File (*.wav)|*.wav|M4A [Stop Using This] (*.m4a)|*.m4a";
            bool? dialogOk = fileDialog.ShowDialog();
            if (dialogOk == true)
            {
                filename = fileDialog.FileName;
                File_Name.Text = filename;
                mediaPlayer.Open(new Uri(filename));
                controls.Visibility = Visibility.Visible;
                file_search.Visibility = Visibility.Hidden;
                youtube_search.Visibility = Visibility.Hidden;
                sync_bar.Visibility = Visibility.Collapsed;
                var tfile = TagLib.File.Create(filename);
                song_title = tfile.Tag.Title;
                song_info = tfile.Tag.JoinedPerformers + " - " + tfile.Tag.JoinedPerformers;
                if (song_title.Length > 22)
                {
                    scroll_title = true;
                    song_title = song_title + "    ";
                    song_title_xaml.Text = song_title.Substring(0, 22);
                }
                else
                {
                    scroll_title = false;
                    song_title_xaml.Text = song_title;
                }
                if (song_info.Length > 32)
                {
                    scroll_info = true;
                    song_info = song_info + "    ";
                    song_info_xaml.Text = song_info;
                }
                else
                {
                    scroll_info = false;
                    song_info_xaml.Text = song_info;
                }
                Open_With_Play();
            }
        }

        async private void BT_Click_Open_Youtube(object sender, RoutedEventArgs e)
        {
            string url_to = url_youtube.Text;
            var video_data = await youtube.Videos.GetAsync(url_to);
            var video = await youtube.Videos.Streams.GetManifestAsync(url_to);
            var audio = video.GetAudioOnly().WithHighestBitrate();
            mediaPlayer.Open(new Uri(audio.Url));
            controls.Visibility = Visibility.Visible;
            file_search.Visibility = Visibility.Hidden;
            youtube_search.Visibility = Visibility.Hidden;
            sync_bar.Visibility = Visibility.Collapsed;
            song_title = video_data.Title;
            song_info = video_data.Author + " - " + video_data.UploadDate.ToString("MM/dd/yyyy");
            if (song_title.Length > 22)
            {
                scroll_title = true;
                song_title = song_title + "    ";
                song_title_xaml.Text = song_title.Substring(0, 22);
            }
            else
            {
                scroll_title = false;
                song_title_xaml.Text = song_title;
            }
            if (song_info.Length > 32)
            {
                scroll_info = true;
                song_info = song_info + "    ";
                song_info_xaml.Text = song_info;
            }
            else
            {
                scroll_info = false;
                song_info_xaml.Text = song_info;
            }
            session = session_name.Text;
            if (session.Length > 0)
            {
                is_syncing = true;
                sync_socket = new WebSocket("wss://socket.sairyonodevs.in/ws/audio/" + session + "/");
                sync_socket.MessageReceived += new EventHandler<MessageReceivedEventArgs>(data_received);
                sync_socket.Open();
                sync_socket.Send("data");
            }
            Open_With_Play();
        }


        private void data_received(object sender, MessageReceivedEventArgs e)
        {
            var obj = JsonConvert.DeserializeObject<Dictionary<string, string>>(e.Message);
            try
            {
                Dictionary<string, string> data = JsonConvert.DeserializeObject<Dictionary<string, string>>(obj["message"]);
                foreach (KeyValuePair<string, string> kvp in data)
                {
                    //textBox3.Text += ("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
                    Debug.WriteLine("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
                }
                if (!is_host)
                {
                    if (data["to_be_updated"] == "1")
                    {
                        slider_to_be_updated = true;
                        Debug.WriteLine("before parse");
                        int time_changed = int.Parse(data["position"]);
                        Debug.WriteLine("before set");
                        update_sync = true;
                        updated_time = time_changed;
                        Debug.WriteLine("after set");
                        slider_to_be_updated = false;
                        is_playing = data["is_playing"];
                    }
                }
            }
            catch
            {
                if (obj["host"] == "true")
                {
                    is_host = true;
                }
                else
                {
                    is_host = false;
                }
            }
        }

        private void BT_Click_Mute(object sender, RoutedEventArgs e)
        {
            if (is_muted)
            {
                if (volume == 0.0)
                {
                    volume = 0.1;
                }
                mediaPlayer.Volume = volume;
                volume_icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.VolumeHigh;
                is_muted = false;
            }
            else
            {
                mediaPlayer.Volume = 0.0;
                volume_icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.VolumeMute;
                is_muted = true;
            }
        }


        private void BT_Click_Play(object sender, RoutedEventArgs e)
        {
            Dictionary<string, string> data;
            string data_str = "";
            int time_changed = (int)(((slider_timer.Value / 10) * totalTime));
            mediaPlayer.Play();
            file_search.Visibility = Visibility.Hidden;
            play_pause_button.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause;
            mediaPlayer.Volume = volume;
            is_playing = "1";
            last_is_playing = "1";
            if (is_host)
            {
                data = new Dictionary<string, string>()
                        {
                            {"is_playing", is_playing },
                            {"to_be_updated", "1"},
                            {"position", time_changed.ToString()},
                        };
                data = new Dictionary<string, string>()
                        {
                            {"message", JsonConvert.SerializeObject(data)}
                        };
                data_str = JsonConvert.SerializeObject(data);
                sync_socket.Send(data_str);
            }
        }

        private void Open_With_Play()
        {
            mediaPlayer.Play();
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += timer_Tick;
            timer.Start();
            file_search.Visibility = Visibility.Hidden;
            play_pause_button.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause;
            mediaPlayer.Volume = volume;
            is_playing = "1";
            last_is_playing = "1";
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (!slider_clicked)
            {
                if (!is_total_time_set)
                {
                    try
                    {
                        totalTime = mediaPlayer.NaturalDuration.TimeSpan.TotalMilliseconds;
                        final_time.Text = mediaPlayer.NaturalDuration.TimeSpan.ToString("hh':'mm':'ss");
                        is_total_time_set = true;
                    }
                    catch (InvalidOperationException e1)
                    {
                        Debug.WriteLine(e1.Message);
                    }
                }
                if (is_syncing)
                {
                    if (!is_host)
                    {
                        if (update_sync)
                        {
                            TimeSpan my_time = new TimeSpan(0, 0, 0, 0, updated_time);
                            mediaPlayer.Position = my_time;
                            curr_time.Text = mediaPlayer.Position.ToString("hh':'mm':'ss");
                            update_sync = false;
                            if (is_playing != last_is_playing)
                            {
                                last_is_playing = is_playing;
                                if (is_playing == "1")
                                {
                                    mediaPlayer.Play();
                                    play_pause_button.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause;
                                }
                                else
                                {
                                    mediaPlayer.Pause();
                                    play_pause_button.Kind = MaterialDesignThemes.Wpf.PackIconKind.Play;
                                }
                            }
                        }
                    }
                }                
                var progress_per = mediaPlayer.Position.TotalMilliseconds / totalTime * 10;
                slider_timer.Value = progress_per;
                curr_time.Text = mediaPlayer.Position.ToString("hh':'mm':'ss");
            }
        }

        private void BT_Click_Reset(object sender, RoutedEventArgs e)
        {
            TimeSpan my_time = new TimeSpan(0, 0, 0, 0, 0);
            mediaPlayer.Position = my_time;
            curr_time.Text = my_time.ToString("hh':'mm':'ss");
        }

        private void BT_Click_Pause(object sender, RoutedEventArgs e)
        {
            Dictionary<string, string> data;
            string data_str = "";
            int time_changed = (int)(((slider_timer.Value / 10) * totalTime));
            mediaPlayer.Pause();
            file_search.Visibility = Visibility.Hidden;
            play_pause_button.Kind = MaterialDesignThemes.Wpf.PackIconKind.Play;
            is_playing = "0";
            last_is_playing = "0";
            if (is_host)
            {
                data = new Dictionary<string, string>()
                        {
                            {"is_playing", is_playing },
                            {"to_be_updated", "1"},
                            {"position", time_changed.ToString()},
                        };
                data = new Dictionary<string, string>()
                        {
                            {"message", JsonConvert.SerializeObject(data)}
                        };
                data_str = JsonConvert.SerializeObject(data);
                sync_socket.Send(data_str);
            }
        }

        private void BT_Click_Win_Close(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }

        private void BT_Click_Win_Min(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void BT_Click_Stop(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Stop();
            filename = "";
            File_Name.Text = filename;
            url_youtube.Text = "";
            mediaPlayer.Close();
            controls.Visibility = Visibility.Hidden;
            file_search.Visibility = Visibility.Visible;
            youtube_search.Visibility = Visibility.Visible;
            sync_bar.Visibility = Visibility.Visible;
            curr_time.Text = "--:--:--";
            final_time.Text = "--:--:--";
            play_not_pause = true;
            play_pause_button.Kind = MaterialDesignThemes.Wpf.PackIconKind.Play;
            is_total_time_set = false;
            slider_to_be_updated = false;
            slider_timer.Value = 0;
        }
    }
}
