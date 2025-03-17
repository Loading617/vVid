use strict;
use warnings;
use Win32::GUI;
use Win32::MediaPlayer;
use Win32::GUI::FileDialog;
use Storable;

my $main = Win32::GUI::Window->new(
    -name   => "Main",
    -title  => "EVideo",
    -width  => 800,
    -height => 600,
);

my $player = Win32::MediaPlayer->new($main, "Player");

my $open_button = $main->AddButton(
    -text    => "Open Video",
    -left    => 10,
    -top     => 10,
    -width   => 100,
    -height  => 30,
    -onClick => \&OpenVideo,
);

my $play_button = $main->AddButton(
    -text    => "Play",
    -left    => 120,
    -top     => 10,
    -width   => 100,
    -height  => 30,
    -onClick => \&PlayVideo,
);

my $pause_button = $main->AddButton(
    -text    => "Pause",
    -left    => 230,
    -top     => 10,
    -width   => 100,
    -height  => 30,
    -onClick => \&PauseVideo,
);

my $rewind_button = $main->AddButton(
    -text    => "Rewind",
    -left    => 340,
    -top     => 10,
    -width   => 100,
    -height  => 30,
    -onClick => \&RewindVideo,
);

my $fast_forward_button = $main->AddButton(
    -text    => "Fast Forward",
    -left    => 450,
    -top     => 10,
    -width   => 100,
    -height  => 30,
    -onClick => \&FastForwardVideo,
);

my $stop_button = $main->AddButton(
    -text    => "Stop",
    -left    => 230,
    -top     => 10,
    -width   => 100,
    -height  => 30,
    -onClick => \&StopVideo,
);

my $volume_slider = $main->AddSlider(
    -left   => 350,
    -top    => 10,
    -width  => 100,
    -height => 30,
    -range  => [0, 100],
    -pos    => 50,
    -onChange => \&SetVolume,
);

my $progress = $main->AddProgressBar(
    -left   => 10,
    -top    => 50,
    -width  => 600,
    -height => 20,
);

my $fullscreen_button = $main->AddButton(
    -text    => "Fullscreen",
    -left    => 460,
    -top     => 10,
    -width   => 100,
    -height  => 30,
    -onClick => \&ToggleFullscreen,
);

my $timer = $main->AddTimer(
    -name    => "UpdateProgress",
    -interval => 1000,
    -onTimer => \&UpdateProgress,
);

sub OpenVideo {
    my $file_dialog = Win32::GUI::GetOpenFileName(
        -title  => "Open Video File",
        -filter => "Video Files (*.mp4;*.avi;*.wmv)\0*.mp4;*.avi;*.wmv\0",
    );
    
    if ($file_dialog) {
        $player->SetFile($file_dialog);
    }
}

sub PlayVideo {
    $player->Play() if $player->GetFile();
}

sub StopVideo {
    $player->Stop();
    $progress->SetPos(0);
}

sub SetVolume {
    my $volume = $volume_slider->GetPos();
    $player->SetVolume($volume);
}

sub ToggleFullscreen {
    $player->FullScreen(!$player->FullScreen());
}

sub UpdateProgress {
    if ($player->GetFile() && $player->GetLength() > 0) {
        my $position = ($player->GetPosition() / $player->GetLength()) * 100;
        $progress->SetPos($position);
        
        store { file => $player->GetFile(), position => $player->GetPosition() }, $last_position_file;
    }
}

sub AddToPlaylist {
    my $file_dialog = Win32::GUI::GetOpenFileName(
        -title  => "Add Video to Playlist",
        -filter => "Video Files (*.mp4;*.avi;*.wmv)\0*.mp4;*.avi;*.wmv\0",
    );
    
    if ($file_dialog) {
        push @playlist, $file_dialog;
        $playlist_box->AddString($file_dialog);
    }
}

$main->Show();
Win32::GUI::Dialog();
