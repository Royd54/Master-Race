using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour {

	[SerializeField] private Queue<string> _Sentences;

    [SerializeField] private Queue<AudioClip> _AudioClips;
    [SerializeField] private Text _dialogueText;
    [SerializeField] private AudioSource _audioSource;

    [SerializeField] private Animator _animator;

    public bool started = false;

	// Use this for initialization
	void Start () {
		_Sentences=new Queue<string>();
		_AudioClips = new Queue<AudioClip>();
	}

    private void Update()
    {
        if (started ==true && _audioSource.isPlaying == false) StartCoroutine(cooldown());
    }

    public void StartDialogue(Dialogue dialogue){
        started = true;
        _animator.SetBool("IsOpen",true);
		_Sentences.Clear();
		_AudioClips.Clear();

		foreach(string sentence in dialogue.sentences){
			_Sentences.Enqueue(sentence);
		}
		foreach(AudioClip audioClip in dialogue.audioClips){
			_AudioClips.Enqueue(audioClip);
		}
        DisplayNextSentence();
	}

    private IEnumerator cooldown()
    {
        yield return new WaitForSeconds(1);
        DisplayNextSentence();
    }

    private void DisplayNextSentence()
    {
        if (_Sentences.Count == 0)
        {
            EndDialogue();
            return;
        }
        string sentence = _Sentences.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));

        //Play Audio Clip Here
        AudioClip audioclip = _AudioClips.Dequeue();
        _audioSource.Stop();
        _audioSource.clip = audioclip;
        _audioSource.Play();
    }

	IEnumerator TypeSentence(string sentence){
		_dialogueText.text="";
		foreach(char letter in sentence.ToCharArray()){
			_dialogueText.text+=letter;
			yield return null;
		}
	}
	
	void EndDialogue(){
		_animator.SetBool("IsOpen",false);
	}
}
