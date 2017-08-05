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

        return;

        allowFire = true;
        ShipController.StepArrow.color = new Color(0f, 255f, 0f);

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

        // отправляем информацию о попаданиях/промахах
        for (int j = yL; j <= yR; j++)
            for (int i = xL; i <= xR; i++)
            {
                try
                {
                    if (ships[i, j] == 1)
                    {
                        myBg.BattleFieldUpdater(i, j, true);
                        //photonView.RPC("TargetHitting", PhotonTargets.Others, (byte)((i << 4) | j), true);
                    }
                    else
                    {
                        myBg.BattleFieldUpdater(i, j, false);
                        //photonView.RPC("TargetHitting", PhotonTargets.Others, (byte)((i << 4) | j), false);
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
        yield return new WaitForSeconds(.1f);
        base.StartPlay();
    }
}
