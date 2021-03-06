using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingPanel : MonoBehaviour
{
    public bool IsOpen { get; set; }
    private Animator _animator;

    public void Open()
    {
        if (IsOpen)
            return;
        if(UIController.Instance.IsQuestTrackOpen())
            UIController.Instance.CloseQuestTrack();
        _animator.SetTrigger("Open");
        IsOpen = true;
    }

    public void Close()
    {
        if (!IsOpen)
            return;
        if (!UIController.Instance.IsQuestTrackOpen())
            UIController.Instance.OpenQuestTrack();
        _animator.SetTrigger("Close");
        IsOpen=false;
    }

    private void Awake()
    {
        _animator = GetComponent<Animator>();

    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name.Equals("Game"))
        {
            IsOpen = true;
            Close();
        }
    }
}
