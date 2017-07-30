﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class InitializeUser : Photon.MonoBehaviour
{
    private PhotonView PhotonView;
    
    public static int[,] ships;             // позиции кораблей
    ShipSortingScene ShipController;        // необходим для извлечения инфо о списке кораблей

    public static bool isReady;             // нажата ли кнопка далее
    private bool gameStart;                 // старт игры когда все будут готовы

    private float timer = 2f;               // таймер для задержки на проверку готовы ли все игроки

    private Battleground enemyBg;           // поле отображающее попадания/промахи по врагу

    public static ShootingArea ShootingArea;

    public static bool allowFire;           // доступность стрельбы в очереди

    private void Awake()
    {
        PhotonView = GetComponent<PhotonView>();
        ShipController = FindObjectOfType<ShipSortingScene>();        
    }

    private void Start()
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

    private void Update()
    {
        timer -= Time.deltaTime;
        if (!gameStart && isReady && PhotonView.isMine && timer <= 0f)
        {
            timer = 2f;
            photonView.RPC("StartPlayChecking", PhotonTargets.Others);

            /* ---- ПОКА ДЕБАГГИНГ ---- */
            //ShipController.SetShipPosPanel.SetActive(false);
            //ShipController.BattleSceneCanvas.SetActive(true);
            //ShipController.EnemyField.SetActive(true);
            //ShipController.WaitingText.gameObject.SetActive(false);

            //gameStart = true;
            /* ---- ПОСЛЕ УДАЛИТЬ!! ---- */
        }       

        if (gameStart && allowFire)
            ShootsFire();
    }
    
    private void ShootsFire()
    {        
        if (PhotonView.isMine && Input.GetMouseButtonUp(0))
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
                return;
            
            allowFire = false;
            ShipController.StepArrow.color = new Color(255f, 0f, 0f);

            // записываем в порядке от левого нижнего угла к правому верхнему
            int packed_data = (xL << 12) | (yL << 8) | (xR << 4) | yR;
            print("data size: " + BitConverter.GetBytes(packed_data).Length);
            photonView.RPC("ModifiedFire", PhotonTargets.Others, packed_data);
        }
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
    private void StartPlay()
    {
        ShipController.SetShipPosPanel.SetActive(false);
        ShipController.BattleSceneCanvas.SetActive(true);
        ShipController.EnemyField.SetActive(true);
        ShipController.WaitingText.gameObject.SetActive(false);
        ShipController.StepArrow.gameObject.SetActive(true);
        gameStart = true;

        ShootingArea = new ShootingArea();  // по дефолту выстрелы размером 1х1

        enemyBg = GameObject.Find("Enemy_field").GetComponent<Battleground>();
    }

    // +++ ВЫСТРЕЛ +++
    [PunRPC]        
    private void ModifiedFire(int packed_data)
    {
        byte mask = 15;         // маска 0000 1111
        
        print("Hit area: " + ((packed_data >> 12) & mask) + ", " + ((packed_data >> 8) & mask) + " | " + ((packed_data >> 4) & mask) + ", " + (packed_data & mask));

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
                    if (ships[i, j] == 1)
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
        /* информация о кораблях
        foreach (Ship sh in ShipController.ShipListing)
        {
            foreach (ShipCoords coords in sh.Coords)
            {
                if (coords.x == x && coords.y == y && sh.ShootsRemaining > 0)
                {
                    print("HiiiiiiiiiiiiiT! " + coords.x + ", " + coords.y + " | Lives " + sh.ShootsRemaining);
                    sh.ShootsRemaining--;
                }
                else if (coords.x == x && coords.y == y && sh.ShootsRemaining == 0)
                {
                    print("YOU DEAD MAAAN!!!");
                }
            }
        }
        */

        print(ships[x, y] + " | Original pos " + ShipController.ShipFieldPos[x, y]);

        if (ships[x, y] == 1)
            return true;
        
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
}
