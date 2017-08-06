using System;
using System.Collections;
using UnityEngine;

public class SinglePlayer : InitializeUser
{
    private int[,] enemyShips;          // корабли компьютера

    private Battleground myBg;

    public delegate void StartSinglePlay();
    public static StartSinglePlay OnClickNext;          // Если стартует в одиночной игре

    private int AiShipsLeft = 10;
    private int UsrShipsLeft = 10;

    protected override void Awake()
    {
        base.Awake();
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
        
        // отмечаем попаданиях/промахах
        for (int j = yL; j <= yR; j++)
            for (int i = xL; i <= xR; i++)
            {
                try
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
                                            Debug.LogError("Ai Ship: " + ShipController.AIShipListing[index].Size + " died!");
                                        }

                                    }
                                }
                            }
                        }
                        enemyBg.BattleFieldUpdater(i, j, true);
                        allowFire = true;
                        ShipController.StepArrow.color = new Color(0f, 255f, 0f);
                        if (AiShipsLeft == 0)
                        {
                            Debug.LogError("!!!YOU WON!!");
                        }
                    }
                    else
                    {
                        enemyBg.BattleFieldUpdater(i, j, false);
                        StartCoroutine(AiShootingCooldown());
                    }
                }
                catch (Exception ex)
                {
                    print(ex.Message);
                }
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
        int shootX, shootY;
        shootX = UnityEngine.Random.Range(0, 10);
        shootY = UnityEngine.Random.Range(0, 10);

        // если по указанным координатам комп уже стрелял - повторить попытку
        if (myBg.BattleFieldArray[shootX, shootY] == 0 || myBg.BattleFieldArray[shootX, shootY] == 1)
            AIshooting();

        if (ships[shootX, shootY] == 1)             // при попадании - АИ стреляет ещё разок
        {
            for (int index = 0; index < ShipController.ShipListing.Count; index++)
            {
                if (ShipController.ShipListing[index].IsAlive)
                {
                    foreach (ShipCoords shCoord in ShipController.ShipListing[index].Coords)
                    {
                        if (shCoord.x == shootX && shCoord.y == shootY)
                        {
                            ShipController.ShipListing[index].ShootsRemaining--;
                            if (ShipController.ShipListing[index].ShootsRemaining == 0)
                            {
                                ShipController.ShipListing[index].IsAlive = false;
                                UsrShipsLeft--;
                                Debug.LogError("user Ship: " + ShipController.ShipListing[index].Size + " died!");
                            }
                        }
                    }
                }
            }
            allowFire = false;
            myBg.BattleFieldUpdater(shootX, shootY, true);
            if (UsrShipsLeft == 0)
            {
                Debug.LogError("!!!AI WON!!");
            }
            AIshooting();
        }
        else
        {
            allowFire = true;
            myBg.BattleFieldUpdater(shootX, shootY, false);
            ShipController.StepArrow.color = new Color(0f, 255f, 0f);
        }
    }
}
