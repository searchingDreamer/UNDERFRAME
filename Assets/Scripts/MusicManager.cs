using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    static public MusicManager Instance;

    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioClip menuSound;
    [SerializeField] AudioClip shopTheme;
    [SerializeField] List<AudioClip> locationTheme;

    private Coroutine musicCoroutine;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);

        DontDestroyOnLoad(Instance);
    }

    void Start()
    {
        musicSource.clip = menuSound;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
        musicSource.Play();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(scene.name == "underframe")
        {
            musicCoroutine = StartCoroutine(MainTheme());
        }
        else if(scene.name == "shop")
        {
            StopCoroutine(musicCoroutine);
            musicSource.time = 0f;
            musicSource.Stop();
            musicSource.clip = shopTheme;
            musicSource.Play();
        }
        else if(scene.name == "loading")
        {
            musicSource.Stop();
        }
    }

    private IEnumerator MainTheme()
    {
        musicSource.time = 2f;
        while(true)
        {
            int songToPlay = Random.Range(0, locationTheme.Count);
            float songLenght = locationTheme[songToPlay].length;
            musicSource.Stop();
            musicSource.clip = locationTheme[songToPlay];
            musicSource.Play();
            yield return new WaitForSeconds(songLenght);
        }
    }

    public void ChangeVolume(float volume)
    { 
        musicSource.volume = volume; 
    }

    static public void QuitMenu()
    {
        SceneManager.LoadScene("loading");
    }

    static public void ExitGame()
    {
        Application.Quit();
    }
}
