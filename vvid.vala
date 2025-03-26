using Gtk;
using Gst;

class vVid : Application {
    private PlayBin player;
    private VideoWidget video_widget;
    private Scale seek_bar;
    private Scale volume_control;
    private Button play_pause_button, stop_button, open_button, fullscreen_button;
    private bool playing = false, seeking = false;
    private ApplicationWindow window;
    private bool is_fullscreen = false;

    public vVid() {
        Object(application_id: "com.example.FullVideoPlayer", flags: ApplicationFlags.FLAGS_NONE);
    }

    protected override void activate() {
        window = new ApplicationWindow(this) {
            title = "vVid",
            default_width = 900,
            default_height = 600
        };

        Gst.init(null);
        player = new PlayBin("player");
        
        video_widget = new VideoWidget();
        player.set_property("video-sink", video_widget.get_sink());

        seek_bar = new Scale.with_range(Orientation.HORIZONTAL, 0, 100, 1);
        seek_bar.set_draw_value(false);
        seek_bar.set_sensitive(false);
        seek_bar.value_changed.connect(() => {
            if (seeking) {
                player.seek_simple(Format.TIME, SeekFlags.FLUSH, (int64)(seek_bar.value * Gst.SECOND));
            }
        });

        volume_control = new Scale.with_range(Orientation.HORIZONTAL, 0, 1.0, 0.05);
        volume_control.set_value(0.5);
        volume_control.value_changed.connect(() => {
            player.set_property("volume", volume_control.get_value());
        });

        play_pause_button = new Button.with_label("Play");
        stop_button = new Button.with_label("Stop");
        open_button = new Button.with_label("Open");
        fullscreen_button = new Button.with_label("Fullscreen");

        play_pause_button.clicked.connect(toggle_playback);
        rewind_button.clicked.connect(toggle_playback);
        fastforward_button.clicked.connect(toggle_playback);
        stop_button.clicked.connect(stop_playback);
        open_button.clicked.connect(open_file);
        fullscreen_button.clicked.connect(toggle_fullscreen);

        window.key_press_event.connect(on_key_press);

        var controls = new Box(Orientation.HORIZONTAL, 10);
        controls.append(play_pause_button);
        controls.append(stop_button);
        controls.append(open_button);
        controls.append(fullscreen_button);
        controls.append(new Label("Volume:"));
        controls.append(volume_control);

        var vbox = new Box(Orientation.VERTICAL, 0);
        vbox.append(video_widget);
        vbox.append(seek_bar);
        vbox.append(controls);
        window.set_child(vbox);

        window.drag_data_received.connect(on_drag_data_received);
        window.drag_dest_set(DestDefaults.ALL, null, Gdk.DragAction.COPY);

        Timeout.add(500, update_seekbar);

        window.present();
    }

    private void toggle_playback() {
        if (playing) {
            player.set_state(State.PAUSED);
            play_pause_button.label = "Play";
        } else {
            player.set_state(State.PLAYING);
            play_pause_button.label = "Pause";
        }
        playing = !playing;
    }

    private void stop_playback() {
        player.set_state(State.NULL);
        play_pause_button.label = "Play";
        seek_bar.set_value(0);
        playing = false;
    }

    private void open_file() {
        var dialog = new FileChooserDialog("Open Video", window, FileChooserAction.OPEN, 
                                           "Cancel", ResponseType.CANCEL, "Open", ResponseType.ACCEPT);
        dialog.add_filter(new FileFilter() { name = "Videos", add_mime_type("video/*") });

        if (dialog.run() == ResponseType.ACCEPT) {
            player.set_property("uri", "file://" + dialog.get_file().get_path());
            seek_bar.set_sensitive(true);
            toggle_playback();
        }
        dialog.destroy();
    }

    private bool update_seekbar() {
        int64 pos = 0, dur = 0;
        if (player.query_position(Format.TIME, out pos) && player.query_duration(Format.TIME, out dur)) {
            if (!seeking) {
                seek_bar.set_range(0, (double)(dur / Gst.SECOND));
                seek_bar.value = (double)(pos / Gst.SECOND);
            }
        }
        return true;
    }

    private void toggle_fullscreen() {
        if (is_fullscreen) {
            window.unfullscreen();
            fullscreen_button.label = "Fullscreen";
        } else {
            window.fullscreen();
            fullscreen_button.label = "Exit Fullscreen";
        }
        is_fullscreen = !is_fullscreen;
    }

    private bool on_key_press(Widget widget, Gdk.EventKey event) {
        switch (event.keyval) {
            case Gdk.Key.space:
                toggle_playback();
                return true;
            case Gdk.Key.Escape:
                if (is_fullscreen) toggle_fullscreen();
                return true;
            case Gdk.Key.f:
                toggle_fullscreen();
                return true;
        }
        return false;
    }

    private void on_drag_data_received(Widget widget, DragDataReceivedEvent event) {
        var data = event.selection_data.get_text();
        if (data != null) {
            player.set_property("uri", "file://" + data.strip());
            seek_bar.set_sensitive(true);
            toggle_playback();
        }
    }
}

int main(string[] args) {
    var app = new FullVideoPlayer();
    return app.run(args);
}
