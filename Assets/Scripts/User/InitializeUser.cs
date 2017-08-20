using System;
using System.Collections;
using UnityEngine;

public class InitializeUser : Photon.MonoBehaviour
{
    private PhotonView PhotonView;
    
    public static int[,] ships;                 // позиции кораблей
    

    public static bool isReady;                 // нажата ли кнопка далее
    protected bool gameStart;                   // старт игры когда все будут готовы
    public ShipSortingScene ShipController      // необходим для извлечения инфо о списке кораблей
    {
        get; set;
    }  
    private float timer = 2f;                   // таймер для задержки на проверку готовы ли все игроки

    protected Battleground enemyBg;             // поле отображающее попадания/промахи по врагу (присваивается в StartPlay)

    public static bool allowFire;               // доступность стрельбы в очереди
    protected Hashtable playersParams;          // параметры игроков в сессии

    protected virtual void Awake()
    {
        playersParams = new Hashtable();
        try
        {
            PhotonView = GetComponent<PhotonView>();
        }
        catch(Exception ex)
        {
            Debug.LogError(ex.Message);
        }
        ShipController = FindObjectOfType<ShipSortingScene>();        
    }

    protected virtual void Start()
    {
        isReady = false;
        gameStart = false;
        if (PhotonNetwork.isMasterClient)
        {
            ShipController.StepArrow.color = new Color(0f, 255f, 0f);
            allowFire = true;
        }            
        else
        {
            ShipController.StepArrow.color = new Color(255f, 0f, 0f);
            allowFire = false;
        }
            
    }

    protected virtual void Update()
    {
        timer -= Time.deltaTime;
        if (!gameStart && isReady && PhotonView.isMine && timer <= 0f)
        {
            timer = 2f;
            photonView.RPC("StartPlayChecking", PhotonTargets.Others);
        }       

        if (gameStart && allowFire && PhotonView.isMine)
            ShootsFire();
    }
    
    private void ShootsFire()
    {        
        if (Input.GetMouseButtonUp(0))
        {
            int packed_data = InputData();

            if (packed_data == -1)
                return;

            PlayerNetwork.Instance.shootingArea = new ShootingArea();   // после выстрела возвращаем стрельбу в дефолтный размер 1х1

            if (!Arsenal.Instance.reposition)
                Arsenal.Instance.ArsenalPanelReposition();

            if (ShipSortingScene.Instance.currentGunId != 1)
                ShipSortingScene.Instance.gunButtons[ShipSortingScene.Instance.currentGunId - 1].interactable = false;
            photonView.RPC("ModifiedFire", PhotonTargets.Others, packed_data);
        }
    }
    /// <summary>
    /// Корректировка стрельбы по вражине, возвращает -1 если в указанные координаты уже стрелял
    /// </summary>
    protected virtual int InputData()
    {
        Vector2 v2 = Input.mousePosition;
        v2 = Camera.main.ScreenToWorldPoint(v2);

        // если курсор находится за пределами поля стрельбы - ничего не произойдет
        if (v2.x < ShipController.EnemyField.transform.position.x || v2.x > (ShipController.EnemyField.transform.position.x + enemyBg.size_X) ||
            v2.y < ShipController.EnemyField.transform.position.y || v2.y > (ShipController.EnemyField.transform.position.y + enemyBg.size_Y))
            return -1;

        float kx = PlayerNetwork.Instance.shootingArea.sizeX > 1 ? PlayerNetwork.Instance.shootingArea.sizeX / 2f : 0;
        float ky = PlayerNetwork.Instance.shootingArea.sizeY > 1 ? PlayerNetwork.Instance.shootingArea.sizeY / 2f : 0;

        // костыль для орудий с параметрами 2 по широте/высоте
        if (PlayerNetwork.Instance.shootingArea.sizeX == 1) kx = 0.5f;
        if (PlayerNetwork.Instance.shootingArea.sizeY == 1) ky = 0.5f;

        // устанавливаем координаты левого нижнего и правого верхнего угла
        int xL = (int)(v2.x - ShipController.EnemyField.transform.position.x - kx);
        int yL = (int)(v2.y - ky);
        int xR = (int)(v2.x - ShipController.EnemyField.transform.position.x + kx);
        int yR = (int)(v2.y + ky);

        // страхуемся, при выходе за пределы избегаем обработки несуществующих координат
        xL = xL < 0 ? 0 : xL > (enemyBg.size_X - 1) ? (enemyBg.size_X - 1) : xL;
        yL = yL < 0 ? 0 : yL > (enemyBg.size_Y - 1) ? (enemyBg.size_Y - 1) : yL;
        xR = xR < 0 ? 0 : xR > (enemyBg.size_X - 1) ? (enemyBg.size_X - 1) : xR;
        yR = yR < 0 ? 0 : yR > (enemyBg.size_Y - 1) ? (enemyBg.size_Y - 1) : yR;

        // проверка и запрет на стрельбу в уже обстрелянные координаты
        int counter = 0;
        for (int j = yL; j <= yR; j++)
            for (int i = xL; i <= xR; i++)
            {
                if (enemyBg.BattleFieldArray[i, j] == 1 || enemyBg.BattleFieldArray[i, j] == 0)
                    counter++;
            }
        if (((yR - yL) + 1) * ((xR - xL) + 1) == counter)
            return -1;

        allowFire = false;
        ShipController.StepArrow.color = new Color(255f, 0f, 0f);

        // записываем в порядке от левого нижнего угла к правому верхнему
         return ( (xL << 12) | (yL << 8) | (xR << 4) | yR );
    }
    //
    // старт игры когда все игроки нажмут кнопку "готовы"
    //
    [PunRPC]
    private void StartPlayChecking()
    {
        if (isReady)
            photonView.RPC("StartPlay", PhotonTargets.All);
    }
    [PunRPC]
    protected virtual void StartPlay()
    {
        ShipController.SetShipPosPanel.SetActive(false);
        ShipController.BattleSceneCanvas.SetActive(true);
        ShipController.EnemyField.SetActive(true);
        ShipController.StepArrow.gameObject.SetActive(true);
        gameStart = true;

        if (PlayerNetwork.Instance.isMultiplayerGame)
            photonView.RPC("CheckEnemyName", PhotonTargets.Others, PlayerNetwork.Instance.PlayerName);

        enemyBg = GameObject.Find("Enemy_field").GetComponent<Battleground>();
    }
    [PunRPC]
    private void CheckEnemyName(string name)
    {
        ShipController.WaitingText.text = name;
    }

    // +++ ВЫСТРЕЛ +++
    [PunRPC]        
    private void ModifiedFire(int packed_data)
    {
        byte mask = 15;         // маска 0000 1111

        int xL = (packed_data >> 12) & mask;
        int yL = (packed_data >> 8) & mask;
        int xR = (packed_data >> 4) & mask;
        int yR = packed_data & mask;

        Battleground myBg = GameObject.Find("Battle_field").GetComponent<Battleground>();
        
        allowFire = true;
        ShipController.StepArrow.color = new Color(0f, 255f, 0f);

        // стрельба запрещена, если есть хоть одно попадание
        for (int j = yL; j <= yR; j++)
            for (int i = xL; i <= xR; i++)
            {
                if (ships[i, j] == 1)
                {
                    allowFire = false;
                    ShipController.StepArrow.color = new Color(255f, 0f, 0f);
                    photonView.RPC("AllowFiring", PhotonTargets.Others, true);
                    break;
                }
            }
        
        // отправляем информацию о попаданиях/промахах
        for (int j = yL; j <= yR; j++)
            for (int i = xL; i <= xR; i++)
            {
                try
                {
                    if (ShipChecker(i, j))
                    {
                        myBg.BattleFieldUpdater(i, j, true);
                        photonView.RPC("TargetHitting", PhotonTargets.Others, (byte)((i << 4) | j), true);
                    }
                    else
                    {
                        myBg.BattleFieldUpdater(i, j, false);
                        photonView.RPC("TargetHitting", PhotonTargets.Others, (byte)((i << 4) | j), false);
                    }
                }
                catch (Exception ex)
                {
                    print(ex.Message);
                }
            }
    }

    private bool ShipChecker(int x, int y)
    {
        foreach (Ship sh in ShipController.ShipListing)
        {
            foreach (ShipCoords coords in sh.Coords)
            {
                if (coords.x == x && coords.y == y && sh.ShootsRemaining > 0)
                {
                    sh.ShootsRemaining--;
                    
                    if (sh.ShootsRemaining == 0)
                    {
                        ShipController.ShipListing.Remove(sh);
                        if (ShipController.ShipListing.Count == 0)
                        {
                            photonView.RPC("GameOver", PhotonTargets.Others, PlayerNetwork.Instance.PlayerName, ShipController.ShipListing.Count, PlayerPrefs.HasKey("usrExpirience") ? PlayerPrefs.GetInt("usrExpirience") : 0);
                            return true;
                        }
                    }
                    return true;
                }
            }
        }
        
        return false;
    }
    
    [PunRPC]
    private void TargetHitting(byte xy, bool hit)
    {
        byte mask = 15;             // маска
        int X = (xy >> 4) & mask;
        int Y = xy & mask;

        enemyBg.BattleFieldUpdater(X, Y, hit);
    }
    [PunRPC]
    private void AllowFiring(bool allow)    // разрешаем/запрещаем стрельбу
    {
        allowFire = allow;
        ShipController.StepArrow.color = new Color(0f, 255f, 0f);
    }
    [PunRPC]
    private void GameOver(string scndUser, int shipsKilled, int usrExp)
    {
        if (gameStart)
        {
            photonView.RPC("GameOver", PhotonTargets.Others, PlayerNetwork.Instance.PlayerName, ShipController.ShipListing.Count, PlayerPrefs.GetInt("usrExpirience"));
            ShipSortingScene.GameOverEvent();
            gameStart = false;
            GameOverWindow.Instance.GameOver(scndUser, shipsKilled, usrExp);
        }
    }
}
