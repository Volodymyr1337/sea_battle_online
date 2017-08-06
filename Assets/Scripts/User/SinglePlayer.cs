using System;
using System.Collections;
using UnityEngine;

public class SinglePlayer : InitializeUser
{
    private int[,] enemyShips;          // корабли компьютера

    private Battleground myBg;

    public delegate void StartSinglePlay();
    public static StartSinglePlay OnClickNext;          // Если стартует в одиночной игре

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
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    if (enemyShips[i, j] == 1)
                    {
                        enemyBg.BattleFieldUpdater(i, j, true);
                    }
                    else
                    {
                        enemyBg.BattleFieldUpdater(i, j, false);
                    }
                }
            }

            /*
            Vector2 v2 = Input.mousePosition;
            v2 = Camera.main.ScreenToWorldPoint(v2);

            // если курсор находится за пределами поля стрельбы - ничего не произойдет
            if (v2.x < ShipController.EnemyField.transform.position.x || v2.x > (ShipController.EnemyField.transform.position.x + enemyBg.size_X) ||
                v2.y < ShipController.EnemyField.transform.position.y || v2.y > (ShipController.EnemyField.transform.position.y + enemyBg.size_Y))
                return;

            float kx = PlayerNetwork.Instance.shootingArea.sizeX > 1 ? PlayerNetwork.Instance.shootingArea.sizeX / 2f : 0;
            float ky = PlayerNetwork.Instance.shootingArea.sizeY > 1 ? PlayerNetwork.Instance.shootingArea.sizeY / 2f : 0;

            // костыль для орудий с параметрами 2 по широте/высоте
            if (PlayerNetwork.Instance.shootingArea.sizeX == 1) kx = 0.5f;
            if (PlayerNetwork.Instance.shootingArea.sizeY == 1) ky = 0.5f;

            int packed_data = base.InputData(v2, kx, ky);

            if (packed_data == -1)
                return;

            ModifiedFire(packed_data, true);
            */
        }
    }

    private void ModifiedFire(int packed_data, bool human = false)
    {
        byte mask = 15;         // маска 0000 1111

        print("Hit area: " + ((packed_data >> 12) & mask) + ", " + ((packed_data >> 8) & mask) + " | " + ((packed_data >> 4) & mask) + ", " + (packed_data & mask));

        int xL = (packed_data >> 12) & mask;
        int yL = (packed_data >> 8) & mask;
        int xR = (packed_data >> 4) & mask;
        int yR = packed_data & mask;
        
       
        /*
        // стрельба запрещена, если есть хоть одно попадание
        for (int j = yL; j <= yR; j++)
            for (int i = xL; i <= xR; i++)
            {
                if (ships[i, j] == 1 && human)
                {
                    allowFire = false;
                    ShipController.StepArrow.color = new Color(255f, 0f, 0f);
                    // комп стреляет ещё раз
                    break;
                }
            }
*/
        // отмечаем попаданиях/промахах
        for (int j = yL; j <= yR; j++)
            for (int i = xL; i <= xR; i++)
            {
                try
                {
                    if (enemyShips[i, j] == 1)
                    {
                        enemyBg.BattleFieldUpdater(i, j, true);
                        allowFire = true;
                        ShipController.StepArrow.color = new Color(0f, 255f, 0f);
                    }
                    else
                    {
                        enemyBg.BattleFieldUpdater(i, j, false);
                    }
                }
                catch (Exception ex)
                {
                    print(ex.Message);
                }
            }
    }

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
        Debug.LogError("Recursion " + size);
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
        for (int j = 0; j < size; j++)  // установка в позиции
        {
            if (dir < 0.5f)
            {
                if (enemyShips[x + j, y] == 0)
                {
                    enemyShips[x + j, y] = 1;
                    setter++;
                }
            }
            else
            {
                if (enemyShips[x, y + j] == 1)
                {
                    enemyShips[x, y + j] = 1;
                    setter++;
                }
            }
        }
        if (setter == size)
        {
            // Добавить запись кораблей в аррай компьютера
            return 1;
        }
        else
            return -1;
        
    }
}
