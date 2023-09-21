using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Record
{
    public class StartWatchingRecordTest : MonoBehaviour
    {
        public Button PlayBtn;
        public Button PlayBackBtn;
        public Button ReSelectBtn;
        public Button ReStartBtn;
        public Button PasueBtn;
        public Text TimeText;
        private Coroutine PlayCoroutine = null;
        private void Start()
        {
            //设置读取路径和其文件名
            Recorder.Instance.SetSaveOrReadPath(Application.streamingAssetsPath,"Record");
            //设置观看模式
            Recorder.Instance.SetMode(StateType.Watching);

            PlayBtn.onClick.AddListener(Paly);
            PlayBackBtn.onClick.AddListener(PlayBack);
            ReSelectBtn.onClick.AddListener(ReSelect);
            PasueBtn.onClick.AddListener(Pasue);
            ReStartBtn.onClick.AddListener(ReStart);
            ReStartBtn.gameObject.SetActive(false);
        }
        private void OnDestroy()
        {
            PlayBtn.onClick.RemoveAllListeners();
            PlayBackBtn.onClick.RemoveAllListeners();
            ReSelectBtn.onClick.RemoveAllListeners();
            PasueBtn.onClick.RemoveAllListeners();
            ReStartBtn.onClick.RemoveAllListeners();
        }
        private void Update()
        {
            TimeText.text = Recorder.Instance.MasterTime.ToString();
        }

        private void Pasue() 
        {
            if (PlayCoroutine != null) {
                StopCoroutine(PlayCoroutine);
            }
        }
        private void Paly() 
        {
            Pasue();
            PlayCoroutine = StartCoroutine(PlayForward());
        }
        private void PlayBack()
        {
            Pasue();
            PlayCoroutine = StartCoroutine(PlayBackward());
        }
        private void ReSelect() 
        {
            Recorder.Instance.DestoryInstance();
            ReStartBtn.gameObject.SetActive(true);
        }
        private void ReStart() 
        {
            //设置读取路径和其文件名
            Recorder.Instance.SetSaveOrReadPath(Application.streamingAssetsPath, "Record");
            //设置观看模式
            Recorder.Instance.SetMode(StateType.Watching);
            ReStartBtn.gameObject.SetActive(false);
        }




        private IEnumerator PlayForward() 
        {
            while (true)
            {
                Recorder.Instance.StepForwardByFrame();
                yield return new WaitForEndOfFrame();
            }
        }
        private IEnumerator PlayBackward()
        {
            while (true)
            {
                Recorder.Instance.StepBackwardByFrame();
                yield return new WaitForEndOfFrame();
            }
        }


    }
}

