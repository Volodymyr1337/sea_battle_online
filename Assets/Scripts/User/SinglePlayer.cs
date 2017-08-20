using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * - Работает через пень-колоду
 * - зарефакторить
 * - ещё раз проверить ходы АИ, подкрутить соображалку
 */
public class SinglePlayer : InitializeUser
{
    private int[,] enemyShips;          // корабли компьютера

    private Battleground myBg;

    public delegate void StartSinglePlay();
    public static StartSinglePlay OnClickNext;          // Если стартует в одиночной игре

    private int AiShipsLeft = 10;
    private int UsrShipsLeft = 10;
    private List<AiGun> AiGuns = new List<AiGun>();

    // счетчики убитых кораблей компьютера и человека
    private int aiKills;
    private int userKills;
    

    protected override void Awake()
    {
        base.Awake();
        AiGuns.Add(new AiGun(new ShootingArea(0, 0), 99999999, GunName.Default));
        AiGuns.Add(new AiGun(new ShootingArea(2, 0), 1, GunName.Airstrike));
        AiGuns.Add(new AiGun(new ShootingArea(1, 1), 1, GunName.RocketLaunch));
        AiGuns.Add(new AiGun(new ShootingArea(2, 1), 1, GunName.Bombs));
        AiGuns.Add(new AiGun(new ShootingArea(3, 3), 1, GunName.Nuclear));
    }

    protected override void Start()
    {
        isReady = false;
        gameStart = false;
        ShipController.StepArrow.color = new Color(0f, 255f, 0f);
        allowFire = true;
        myBg = GameObject.Find("Battle_field").GetComponent<Battleground>();

        OnClickNext = new StartSinglePlay(StartPlay);

        enemyShips = new int[10, 10];
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                enemyShips[i, j] = 0;
            }
        }
        
    }

    protected override void Update()
    {
        if (gameStart && allowFire)
            ShootsFire();
    }

    private void ShootsFire()
    {
        if (Input.GetMouseButtonUp(0))
        {
            int packed_data = base.InputData();

            if (packed_data == -1)
                return;

            PlayerNetwork.Instance.shootingArea = new ShootingArea();   // после выстрела возвращаем стрельбу в дефолтный размер 1х1
            
            if (!Arsenal.Instance.reposition)
                Arsenal.Instance.ArsenalPanelReposition();
            
            if (ShipSortingScene.Instance.currentGunId != 1)
                ShipSortingScene.Instance.gunButtons[ShipSortingScene.Instance.currentGunId - 1].interactable = false;

            ModifiedFire(packed_data);
        }
    }

    private void ModifiedFire(int packed_data)
    {
        byte mask = 15;         // маска 0000 1111

        int xL = (packed_data >> 12) & mask;
        int yL = (packed_data >> 8) & mask;
        int xR = (packed_data >> 4) & mask;
        int yR = packed_data & mask;


        int usrHits = 0;        // если кол-во попаданий юзера больше 0, то юзер стреляет еще раз
       
        for (int j = yL; j <= yR; j++)       // отмечаем попаданиях/промахах
            for (int i = xL; i <= xR; i++)
            {
                if (enemyShips[i, j] == 1)
                {
                    for (int index = 0; index < ShipController.AIShipListing.Count; index++)
                    {
                        if (ShipController.AIShipListing[index].IsAlive)
                        {
                            foreach (ShipCoords shCoord in ShipController.AIShipListing[index].Coords)
                            {
                                if (shCoord.x == i && shCoord.y == j)
                                {
                                    ShipController.AIShipListing[index].ShootsRemaining--;
                                    if (ShipController.AIShipListing[index].ShootsRemaining == 0)
                                    {
                                        ShipController.AIShipListing[index].IsAlive = false;
                                        AiShipsLeft--;
                                        userKills++;
                                    }

                                }
                            }
                        }
                    }
                    enemyBg.BattleFieldUpdater(i, j, true);
                    usrHits++;

                    if (AiShipsLeft == 0)
                    {
                        ShipSortingScene.GameOverEvent();
                        playersParams.Clear();
                        playersParams.Add("winner", PlayerNetwork.Instance.PlayerName);
                        playersParams.Add("looser", "Android");
                        playersParams.Add("usrKills", userKills.ToString());
                        playersParams.Add("aiKills", aiKills.ToString());
                        playersParams.Add("userExp", (userKills * 2).ToString());
                        playersParams.Add("aiExp", (aiKills * 0.5).ToString());

                        if (PlayerPrefs.HasKey("usrExpirience"))
                        {
                            PlayerPrefs.GetInt("usrExpirience");
                            PlayerPrefs.SetInt("usrExpirience", (PlayerPrefs.GetInt("usrExpirience") + (int)(userKills * 2)));
                        }
                        else
                            PlayerPrefs.SetInt("usrExpirience", userKills);

                        if (PlayerPrefs.HasKey("AiExpirience"))
                        {
                            PlayerPrefs.GetInt("AiExpirience");
                            PlayerPrefs.SetInt("AiExpirience", (PlayerPrefs.GetInt("AiExpirience") + (int)(aiKills * 0.8)));
                        }
                        else
                            PlayerPrefs.SetInt("AiExpirience", aiKills);

                        GameOverWindow.Instance.GameOver(playersParams);
                        return;
                    }
                }
                else
                {
                    enemyBg.BattleFieldUpdater(i, j, false);
                }
            }

        if (usrHits > 0)
        {
            allowFire = true;
            ShipController.StepArrow.color = new Color(0f, 255f, 0f);
        }
        else
        {
            StartCoroutine(AiShootingCooldown());
        }
    }

    #region Start single Play and AI ships spawning

    protected override void StartPlay()
    {
        StartCoroutine(Cooldown());
    }
    IEnumerator Cooldown()
    {
        SpawnComputerShips();

        yield return new WaitForSeconds(.2f);   // чисто стратегический кд

        ShipController.WaitingText.text = "Android";

        base.StartPlay();
    }

    private void SpawnComputerShips()
    {
        for (int i = 0, j = 1; i < 4; i++, j++)
        {
            for (int n = 5; n > j; n--)
            {
                // костыль вместо рекурсии (хз, но рекурсия как будто не успевала или наоборот спаунила лишние корабли)
                int k = 0;
                do
                {
                    k = CompShipPosSetter(j);
                }
                while (k != 1);
            }
           
        }
        
    }

    private int CompShipPosSetter(int size)
    {
        int x, y;
        x = UnityEngine.Random.Range(0, (11 - size));
        y = UnityEngine.Random.Range(0, (11 - size));

        float dir = UnityEngine.Random.Range(0f, 1f);

        int setter = 0;
        
        for (int j = 0; j < size; j++)  // проверка свободные поля или нет
        {
            if (dir < 0.5f)
            {
                if (enemyShips[x + j, y] == 1)
                {
                    return -1;
                }
            }
            else
            {
                if (enemyShips[x, y + j] == 1)
                {
                    return -1;
                }
            }
                
        }

        ShipCoords[] shipCoords = new ShipCoords[size];

        for (int j = 0; j < size; j++)  // установка в позиции
        {
            if (dir < 0.5f)
            {
                if (enemyShips[x + j, y] == 0)
                {
                    shipCoords[j] = new ShipCoords(x + j, y);
                    enemyShips[x + j, y] = 1;
                    setter++;
                }
            }
            else
            {
                if (enemyShips[x, y + j] == 1)
                {
                    shipCoords[j] = new ShipCoords(x, y + j);
                    enemyShips[x, y + j] = 1;
                    setter++;
                }
            }
        }
        if (setter == size)
        {
            // Добавляем корабли АИ в список
            ShipController.AIShipListing.Add(new Ship(shipCoords, true, size));
            return 1;
        }
        else
            return -1;
        
    }

    #endregion

    private IEnumerator AiShootingCooldown()
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(0.5f, 1f));
        AIshooting();
    }

    private void AIshooting()
    {
        int randWeapon = UnityEngine.Random.Range(0, AiGuns.Count);

        int shootX, shootY;
        shootX = UnityEngine.Random.Range(0, (10 - (int)AiGuns[randWeapon].ShootArea.sizeX));
        shootY = UnityEngine.Random.Range(0, (10 - (int)AiGuns[randWeapon].ShootArea.sizeY));

        // если по указанным координатам комп уже стрелял - повторить попытку
        int hitCount = 0;
        for (int i = shootX; i <= (shootX + AiGuns[randWeapon].ShootArea.sizeX); i++)
            for (int j = shootY; j <= (shootY + AiGuns[randWeapon].ShootArea.sizeY); j++)
            {
                if (myBg.BattleFieldArray[i, j] == 0 || myBg.BattleFieldArray[i, j] == 1)
                {
                    hitCount++;
                }
                    
            }
        if (hitCount == (AiGuns[randWeapon].ShootArea.sizeX + 1) * (AiGuns[randWeapon].ShootArea.sizeY + 1))
        {
            AIshooting();
            return;
        }

        int AiHits = 0;     // если кол-во попаданий больше 0 то АИ будет стрелять повторно
        for (int i = shootX; i <= (shootX + AiGuns[randWeapon].ShootArea.sizeX); i++)
            for (int j = shootY; j <= (shootY + AiGuns[randWeapon].ShootArea.sizeY); j++)
            {
                if (ships[i, j] == 1)             // при попадании - АИ стреляет ещё разок
                {
                    for (int index = 0; index < ShipController.ShipListing.Count; index++)
                    {
                        if (ShipController.ShipListing[index].IsAlive)
                        {
                            foreach (ShipCoords shCoord in ShipController.ShipListing[index].Coords)
                            {
                                if (shCoord.x == i && shCoord.y == j)
                                {
                                    ShipController.ShipListing[index].ShootsRemaining--;
                                    if (ShipController.ShipListing[index].ShootsRemaining == 0)
                                    {
                                        ShipController.ShipListing[index].IsAlive = false;
                                        UsrShipsLeft--;
                                        aiKills++;
                                    }
                                }
                            }
                        }
                    }
                    AiHits++;
                    
                    myBg.BattleFieldUpdater(i, j, true);
                    if (UsrShipsLeft == 0)
                    {
                        ShipSortingScene.GameOverEvent();
                        playersParams.Clear();
                        playersParams.Add("winner", "Android");
                        playersParams.Add("looser", "Android");
                        playersParams.Add("usrKills", userKills.ToString());
                        playersParams.Add("aiKills", aiKills.ToString());
                        playersParams.Add("userExp", userKills.ToString());
                        playersParams.Add("aiExp", (aiKills * 2.3).ToString());

                        if (PlayerPrefs.HasKey("usrExpirience"))
                        {
                            PlayerPrefs.GetInt("usrExpirience");
                            PlayerPrefs.SetInt("usrExpirience", (PlayerPrefs.GetInt("usrExpirience") + (int)(userKills * 0.8)));
                        }                            
                        else
                            PlayerPrefs.SetInt("usrExpirience", userKills);

                        if (PlayerPrefs.HasKey("AiExpirience"))
                        {
                            PlayerPrefs.GetInt("AiExpirience");
                            PlayerPrefs.SetInt("AiExpirience", (PlayerPrefs.GetInt("AiExpirience") + (int)(aiKills * 2)));
                        }
                        else
                            PlayerPrefs.SetInt("AiExpirience", aiKills);

                        GameOverWindow.Instance.GameOver(playersParams);
                        return;
                    }                    
                }
                else
                {
                    myBg.BattleFieldUpdater(i, j, false);                   
                }
            }
        // Удаляем валыну если её кол-во = 0
        AiGuns[randWeapon].Count--;
        if (AiGuns[randWeapon].Count == 0)
            AiGuns.RemoveAt(randWeapon);
        
        if (AiHits > 0)
        {
            allowFire = false;
            AIshooting();
        }
        else
        {
            allowFire = true;
            ShipController.StepArrow.color = new Color(0f, 255f, 0f);
        }
    }
}
