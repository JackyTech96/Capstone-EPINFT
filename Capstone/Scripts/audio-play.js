function togglePlay(idNFT) {
    var audio = document.getElementById('audio-nft-' + idNFT);
    var button = document.querySelector('.audio-container .play-button i');
    if (audio.paused) {
        audio.play();
        button.classList.add('bi-pause');
        button.classList.remove('bi-play');
    } else {
        audio.pause();
        button.classList.add('bi-play');
        button.classList.remove('bi-pause');
    }
}