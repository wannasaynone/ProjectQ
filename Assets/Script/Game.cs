using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

public class Game : MonoBehaviour {


    [SerializeField]
    GameObject[] Cups;                                                                                    //杯子
    Vector3[] AbsolutePositionSet;                                                                        //遊戲初始設定絕對位置
    [SerializeField]
    int RandomMoveTimes;                                                                                  //在編輯器中設置起始隨機移動次數
    int randomMoveTimesNow;                                                                               //目前的隨機移動次數，隨著等級變化
    [SerializeField]
    float MoveDuration;                                                                                   //再編輯器中設置起始移動間隔
    float moveDurationNow;                                                                                //目前的移動間隔，隨著等級變化
    List<Vector3> tempPosition;                                                                           //隨機排列使用，暫存目前隨機排列內容
    public static int AsignGoalToWhichCup;                                                                //目前目標在哪一個杯子
    int Level = 0;                                                                                        //目前的等級

    [SerializeField]
    GameObject[] CupsImages;                                                                              //杯子在畫面上的圖案
    [SerializeField]
    GameObject Fish;                                                                                      //飼料圖
    Image FishSkin;
    RectTransform FishSkinRectTransform;
    List<Button> CupsButton;                                                                              //杯子圖上的按鈕功能

    [SerializeField]
    Text LevelText;                                                                                       //顯示目前等級的文字
    [SerializeField]
    GameObject[] LeftCat;                                                                                 //圖片組:左邊的貓
    [SerializeField]
    GameObject[] MiddleCat;                                                                               //圖片組:中間的貓
    [SerializeField]
    GameObject[] RightCat;                                                                                //圖片組:右邊的貓
    [SerializeField]
    Image[] CatsReact;                                                                                    //圖片組:左中右的貓臉，替換表情使用
    [SerializeField]
    GameObject StandByCat;                                                                                //圖片組:待機中的貓
    CatEyeMove standByCatEyeMove;                                                                         //待機貓帶的眼球移動動態

    [SerializeField]
    GameObject AgainButton;                                                                               //再來一次的按鈕

    AudioSource se;

    bool ActualMoved = false;

    private void Start()//初始化
    {
        AbsolutePositionSet = new Vector3[Cups.Length];                                                   //杯子的絕對位置數量 = 杯子的數量
        for (int i = 0; i < AbsolutePositionSet.Length; i++)                                              //初始化設定絕對位置 = 目前的杯子位置
        {
            AbsolutePositionSet[i] = 
                new Vector3(Cups[i].transform.localPosition.x, Cups[i].transform.localPosition.y);
        }
        CupsButton = new List<Button>();                                                                  //初始化杯子按鈕清單
        for(int i = 0; i < CupsImages.Length; i++)                                                        //實做按鈕
        {
            CupsButton.Add(CupsImages[i].GetComponent<Button>());
        }

        moveDurationNow = MoveDuration;                                                                   //初始化移動間隔時間
        randomMoveTimesNow = RandomMoveTimes;                                                             //初始化隨機移動次數

        standByCatEyeMove = StandByCat.GetComponent<CatEyeMove>();                                        //實做待機貓的眼球移動動態

        FishSkin = Fish.GetComponent<Image>();
        FishSkinRectTransform = Fish.GetComponent<RectTransform>();

        LevelText.text = "Level " + Level;                                                                //初始化等級文字

        AsignGoalToWhichCup = 0;                                                                          //初始化目標目前位置(在0號杯)

        se = GameObject.Find("SE").GetComponent<AudioSource>();

        tempPosition = new List<Vector3>();

        for(int i = 0; i < Cups.Length;i++)
        {
            tempPosition.Add(Cups[i].transform.position);
        }

        GameStart();                                                                                      //開始遊戲

    }

    public void GameStart()//開始遊戲
    {
        //AsignGoalToWhichCup = Random.Range(0, Cups.Length);

        //LevelText.text = "Level " + Level;                                                                 //更新等級文字

        if (moveDurationNow > 0.05)                                                                        //每次開始遊戲設定目前的移動間隔
            moveDurationNow = MoveDuration - (0.02f * Level);

        if (randomMoveTimesNow < 100)                                                                      //並設定隨機移動次數
            randomMoveTimesNow = RandomMoveTimes + Level;

        AllButtonDisable();                                                                                //遊戲開始時讓所有按鈕失去功能

        for (int m = 0; m < CupsImages.Length; m++)                                                        //遊戲開始時執行所有杯子升起的動態
        {
            //Debug.Log(m);
            CupsImages[m].transform.DOLocalMoveY(100f, moveDurationNow);
        }

        StartCoroutine(StartMovingCups());                                                                 //接著開始執行杯子移動動態
    }

    IEnumerator StartMovingCups()
    {
        standByCatEyeMove.EyeMove();                                                                       //執行眼球移動的動態

        yield return new WaitForSeconds(1.5f);

        for (int m = 0; m < CupsImages.Length; m++)                                                        //執行杯子蓋下的動態
        {
            //Debug.Log(m);
            CupsImages[m].transform.DOLocalMoveY(0f, moveDurationNow);
        }

        yield return new WaitForSeconds(moveDurationNow);                                                  //動完之後

        StartCoroutine(RandomMove(randomMoveTimesNow));                                                    //先隨機動(目前隨機移動次數)次

        yield return new WaitForSeconds(randomMoveTimesNow * moveDurationNow);                             //動完之後

        RandomSortPosition();                                                                              //骰得排列方式

        for (int m = 0; m < Cups.Length; m++)                                                              //並排列他們
        {
            Cups[m].transform.DOLocalMove(tempPosition[m], moveDurationNow);
        }

        if (ActualMoved)
        {
            se.clip = Resources.Load<AudioClip>("sounds/sliding");
            se.Play();
        }

        yield return new WaitForSeconds(moveDurationNow);                                                  //動完之後

        AllButtonEnable();                                                                                 //開啟所有按鈕
    }

    IEnumerator RandomMove(int MoveTimes)//隨機移動(動幾次)
    {
        for (int i = 0; i < MoveTimes; i++)                                                                //每一次隨機移動
        {
            RandomSortPosition();                                                                          //就隨機排列一次
            for (int m = 0; m < Cups.Length; m++)
            {  
                Cups[m].transform.DOLocalMove(tempPosition[m], moveDurationNow);                           //然後將杯子放到定位
            }

            if(ActualMoved)
            {
                se.clip = Resources.Load<AudioClip>("sounds/sliding");
                se.Play();
            }

            yield return new WaitForSeconds(moveDurationNow);                                              //動完之後再進行一次隨機移動
            
        }
    }

    public void SelectCup(int ThisIsWhichCup)//按鈕功能：目前玩家按下的是第幾號杯子(杯子的絕對編號)
    {
        se.clip = Resources.Load<AudioClip>("sounds/mouse-click");
        se.Play();

        int CheckDisplayWhichCat = 4;                                                                      //計算要顯示的是左中右哪隻貓

        if (Cups[ThisIsWhichCup].transform.localPosition.x < -100f)                                        //如果這個杯子的位置在左邊，顯示左邊的
            CheckDisplayWhichCat = 0;
        else if (Cups[ThisIsWhichCup].transform.localPosition.x > 100f)                                    //在右邊就顯示右邊的
            CheckDisplayWhichCat = 2;
        else
            CheckDisplayWhichCat = 1;                                                                      //左右都不是就顯示中間的

        AllButtonDisable();                                                                                //選擇杯子後要將所有的杯子按鈕取消掉

        StandByCat.SetActive(false);                                                                       //然後把待機貓也關掉

        StartCoroutine(Answer(ThisIsWhichCup, CheckDisplayWhichCat));                                      //執行回答
    }

    IEnumerator Answer(int ThisIsWhichCup, int CheckDisplayWhichCat)//回答，顯示答案結果(選了哪個杯子，要顯示哪隻貓)
    {
        switch (CheckDisplayWhichCat)                                                                      //根據要顯示哪隻貓決定顯示左中右哪個貓
        {
            case 0://左
                for(int i = 0; i < LeftCat.Length; i++)
                {
                    LeftCat[i].SetActive(true);
                }
                LeftCat[1].transform.DOLocalMoveY(-100f, MoveDuration);                                     //並執行手向下準備將杯子拿起的動畫
                LeftCat[2].transform.DOLocalMoveY(-100f, MoveDuration);
                break;
            case 1://中
                for (int i = 0; i < MiddleCat.Length; i++)
                {
                    MiddleCat[i].SetActive(true);
                }
                MiddleCat[1].transform.DOLocalMoveY(-100f, MoveDuration);
                MiddleCat[2].transform.DOLocalMoveY(-100f, MoveDuration);
                break;
            case 2://右
                for (int i = 0; i < RightCat.Length; i++)
                {
                    RightCat[i].SetActive(true);
                }
                RightCat[1].transform.DOLocalMoveY(-100f, MoveDuration);
                RightCat[2].transform.DOLocalMoveY(-100f, MoveDuration);
                break;
        }

        yield return new WaitForSeconds(MoveDuration);                                                      //動完之後

        switch (CheckDisplayWhichCat)                                                                       
        {
            case 0://左
                LeftCat[1].transform.DOLocalMoveY(0f, MoveDuration);                                        //執行手向上將杯子拿起的動畫
                LeftCat[2].transform.DOLocalMoveY(0f, MoveDuration);
                break;
            case 1://中
                MiddleCat[1].transform.DOLocalMoveY(0f, MoveDuration);
                MiddleCat[2].transform.DOLocalMoveY(0f, MoveDuration);
                break;
            case 2://右
                RightCat[1].transform.DOLocalMoveY(0f, MoveDuration);
                RightCat[2].transform.DOLocalMoveY(0f, MoveDuration);
                break;
        }

        if (ThisIsWhichCup == AsignGoalToWhichCup)                                                          //如果選擇的杯子是正確答案
        {
            CupsImages[ThisIsWhichCup].transform.DOLocalMoveY(100f, MoveDuration);                          //就只將選擇的杯子拿起來
            yield return new WaitForSeconds(1f);
            CatsReact[0].sprite = Resources.Load<Sprite>("left/left-win");                                  //並在一秒後變更圖片顯示結果
            CatsReact[1].sprite = Resources.Load<Sprite>("right/right-win");
            CatsReact[2].sprite = Resources.Load<Sprite>("middle/middle-win");
            Level++;                                                                                        //回答正確時升級
        }
        else
        {
            CupsImages[ThisIsWhichCup].transform.DOLocalMoveY(100f, MoveDuration);                          //反之錯誤時先將錯誤的杯子拿起來
            yield return new WaitForSeconds(1.5f);
            CupsImages[AsignGoalToWhichCup].transform.DOLocalMoveY(100f, MoveDuration);                     //1.5秒後正確的杯子升起來
            yield return new WaitForSeconds(1f);
            CatsReact[0].sprite = Resources.Load<Sprite>("left/left-lose");                                 //並在一秒後變更圖片顯示結果
            CatsReact[1].sprite = Resources.Load<Sprite>("right/right-lose");
            CatsReact[2].sprite = Resources.Load<Sprite>("middle/middle-lose");
            Level--;                                                                                        //回答錯誤時降級
        }

        LevelText.text = "Level " + Level;                                                                  //更新等級文字
        se.clip = Resources.Load<AudioClip>("sounds/meow/0" + Random.Range(1, 8));
        se.Play();
        AgainButton.SetActive(true);                                                                        //將「再一次」的按鈕打開
    }

    public void Restart()//按鈕功能：重新開始遊戲
    {
        se.clip = Resources.Load<AudioClip>("sounds/mouse-click");
        se.Play();
        StartCoroutine(GameRestart());                                                                        //執行重新開始遊戲
    }

    IEnumerator GameRestart()//重新開始遊戲
    {
        int fishSkinRoll = Random.Range(0, 6);

        FishSkin.sprite = Resources.Load<Sprite>("fish/" + fishSkinRoll.ToString());

        switch(fishSkinRoll)
        {
            case 0:
                FishSkinRectTransform.sizeDelta = new Vector2(145f, 60f);
                Fish.transform.localPosition = new Vector3(0f, -50f);
                break;
            case 1:
                FishSkinRectTransform.sizeDelta = new Vector2(145f, 60f);
                Fish.transform.localPosition = new Vector3(0f, -50f);
                break;
            case 2:
                FishSkinRectTransform.sizeDelta = new Vector2(82f, 81f);
                Fish.transform.localPosition = new Vector3(0f, -40f);
                break;
            case 3:
                FishSkinRectTransform.sizeDelta = new Vector2(76f, 90f);
                Fish.transform.localPosition = new Vector3(0f, -40f);
                break;
            case 4:
                FishSkinRectTransform.sizeDelta = new Vector2(116f, 44f);
                Fish.transform.localPosition = new Vector3(0f, -50f);
                break;
            case 5:
                FishSkinRectTransform.sizeDelta = new Vector2(81f, 57f);
                Fish.transform.localPosition = new Vector3(0f, -55f);
                break;

        }

        AgainButton.SetActive(false);                                                                         //將再一次的按鈕關閉

        for (int i = 0; i < LeftCat.Length; i++)                                                              //將左中右貓都關閉
        {
            LeftCat[i].SetActive(false);
        }

        for (int i = 0; i < RightCat.Length; i++)
        {
            RightCat[i].SetActive(false);
        }

        for (int i = 0; i < MiddleCat.Length; i++)
        {
            MiddleCat[i].SetActive(false);
        }

        CatsReact[0].sprite = Resources.Load<Sprite>("left/left");                                           //並將左中右貓的表情恢復原狀
        CatsReact[1].sprite = Resources.Load<Sprite>("right/right");
        CatsReact[2].sprite = Resources.Load<Sprite>("middle/middle");

        StandByCat.SetActive(true);                                                                          //開啟待機貓
        standByCatEyeMove.StopEyeMove();                                                                     //並讓貓的眼球停止移動

        for (int m = 0; m < CupsImages.Length; m++)                                                          //讓所有杯子都升起
        {
            //Debug.Log(m);
            CupsImages[m].transform.DOLocalMoveY(100f, MoveDuration);
        }
        yield return new WaitForSeconds(MoveDuration);                                                       //動完之後
        GameStart();                                                                                         //遊戲開始
    }

    void AllButtonDisable()//所有按鈕關閉功能
    {
        for(int i = 0; i < CupsButton.Count; i++)
        {
            CupsButton[i].enabled = false;
        }
    }

    void AllButtonEnable()//所有按鈕開啟功能
    {
        for (int i = 0; i < CupsButton.Count; i++)
        {
            CupsButton[i].enabled = true;
        }
    }

    void RandomSortPosition()//骰得隨機杯子位置
    {
        List<Vector3> oldPos = new List<Vector3>();
        for (int i = 0; i < tempPosition.Count; i++)
        {
            oldPos.Add(tempPosition[i]);
        }

        List<int> randomList = new List<int>();                                                              //設定骰池基數(要骰幾次)
        List<Vector3> sortingCups = new List<Vector3>();                                                     //設定新的隨機排列用清單

        for (int m = 0; m < Cups.Length; m++)                                                                //有幾個杯子，骰池基數就要有幾個
        {
            randomList.Add(m);
        }

        while (randomList.Count > 0)                                                                          //只要骰池積數還沒被清空
        {
            int roll = Random.Range(0, randomList.Count);                                                     //骰得一個基數
            sortingCups.Add(AbsolutePositionSet[randomList[roll]]);                                           //將這個基數代表的位置分配到新的隨機排列用清單
            randomList.RemoveAt(roll);                                                                        //然後將這個基數從清單中刪除(表示已分配)
        }

        tempPosition = sortingCups;                                                                           //根據亂數骰定結果更新暫存清單

        int checkedNum = 0;

        if (oldPos.Count > 0)
        {
            for (int i = 0; i < oldPos.Count; i++)
            {
                if (oldPos[i] == tempPosition[i])
                {
                    checkedNum++;
                }

                if (checkedNum == oldPos.Count)
                {
                    ActualMoved = false;
                }
                else
                {
                    ActualMoved = true;
                }
            }
        }

    }
}
