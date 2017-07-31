using System;
using UnityEngine;

public class SingleUser : InitializeUser
{
    private int[,] enemyShips;          // корабли компьютера

    private Battleground myBg;
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

        StartPlay();
    }

    private void Update()
    {
        if (gameStart && allowFire)
            ShootsFire();
    }

    private void ShootsFire()
    {
        if (Input.GetMouseButtonUp(0))
        {
            int packed_data = InputData();

            if (packed_data == -1)
                return;

            ModifiedFire(packed_data, true);
        }
    }

    private void ModifiedFire(int packed_data, bool human)
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

    protected override int InputData()
    {
        return base.InputData();
    }

    protected override void StartPlay()
    {
        base.StartPlay();
    }
}
