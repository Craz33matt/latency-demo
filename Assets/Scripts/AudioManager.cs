using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Music")]
    public AudioSource musicSource;
    public AudioClip[] menuTracks;
    [Range(0f, 1f)] public float[] menuTrackVolumes;
    public AudioClip[] gameTracks;
    [Range(0f, 1f)] public float[] gameTrackVolumes;
    public bool shuffleTracks = false;

    [Header("SFX")]
    public AudioSource sfxSource;
    public AudioClip buttonHoverClip;
    public bool autoHookButtons = true;

    private AudioClip[] currentPlaylist;
    private float[] currentPlaylistVolumes;
    private int currentTrackIndex = -1;

    private void OnValidate()
    {
        ResizeVolumeArray(ref menuTrackVolumes, menuTracks);
        ResizeVolumeArray(ref gameTrackVolumes, gameTracks);

        ApplyCurrentTrackVolume();
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (musicSource == null)
            musicSource = gameObject.AddComponent<AudioSource>();

        if (sfxSource == null)
            sfxSource = gameObject.AddComponent<AudioSource>();

        musicSource.loop = false;
        sfxSource.loop = false;
    }

    private void Start()
    {
        if (autoHookButtons)
            HookButtonsInScene();

        PlayMenuMusic();
    }

    private void Update()
    {
        if (currentPlaylist == null || currentPlaylist.Length == 0)
            return;

        ApplyCurrentTrackVolume();

        if (!musicSource.isPlaying)
            PlayNextTrack();
    }

    public void PlayMenuMusic()
    {
        PlayPlaylist(menuTracks);
    }

    public void PlayGameMusic()
    {
        PlayPlaylist(gameTracks);
    }

    public void PlayButtonHover()
    {
        if (buttonHoverClip == null || sfxSource == null)
            return;

        sfxSource.PlayOneShot(buttonHoverClip);
    }

    public void HookButtonsInScene()
    {
        Button[] buttons = FindObjectsOfType<Button>(true);

        foreach (Button button in buttons)
        {
            ButtonHoverSound hoverSound = button.GetComponent<ButtonHoverSound>();

            if (hoverSound == null)
                hoverSound = button.gameObject.AddComponent<ButtonHoverSound>();

            hoverSound.audioManager = this;
        }
    }

    private void PlayPlaylist(AudioClip[] playlist)
    {
        if (playlist == null || playlist.Length == 0 || musicSource == null)
            return;

        if (currentPlaylist == playlist && musicSource.isPlaying)
            return;

        currentPlaylist = playlist;
        currentPlaylistVolumes = playlist == menuTracks ? menuTrackVolumes : gameTrackVolumes;
        currentTrackIndex = -1;
        PlayNextTrack();
    }

    private void PlayNextTrack()
    {
        if (currentPlaylist == null || currentPlaylist.Length == 0)
            return;

        if (shuffleTracks)
        {
            currentTrackIndex = Random.Range(0, currentPlaylist.Length);
        }
        else
        {
            currentTrackIndex = (currentTrackIndex + 1) % currentPlaylist.Length;
        }

        musicSource.clip = currentPlaylist[currentTrackIndex];
        ApplyCurrentTrackVolume();
        musicSource.Play();
    }

    private void ApplyCurrentTrackVolume()
    {
        if (musicSource == null)
            return;

        musicSource.volume = GetCurrentTrackVolume();
    }

    private float GetCurrentTrackVolume()
    {
        if (currentTrackIndex < 0 || currentPlaylistVolumes == null || currentTrackIndex >= currentPlaylistVolumes.Length)
            return 1f;

        return currentPlaylistVolumes[currentTrackIndex];
    }

    private void ResizeVolumeArray(ref float[] volumeArray, AudioClip[] trackArray)
    {
        int length = trackArray == null ? 0 : trackArray.Length;

        if (volumeArray != null && volumeArray.Length == length)
            return;

        float[] newVolumes = new float[length];

        for (int i = 0; i < newVolumes.Length; i++)
        {
            newVolumes[i] = volumeArray != null && i < volumeArray.Length ? volumeArray[i] : 1f;
        }

        volumeArray = newVolumes;
    }
}
