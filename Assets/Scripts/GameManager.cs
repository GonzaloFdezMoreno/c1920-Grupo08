﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    const float playerMaxHP = 200f;
    float shieldMaxHP = 100f;
    float playerHP, shieldHP, shieldWeight; // Estado del juego
    float checkpointPlayerHP, checkpointShieldHP, checkpointShieldWeight; // Variables de puntos de guardados
    Vector2 lastCheckpoint; // Lugar donde reaparecerá el jugador al morir
    Sprite checkpointShield; // Escudo que tenía el jugador al pasar por el checkpoint
    GameObject player, shield;
    //True cuando el jugador acaba de recibir daño y es brevemente inmune al daño
    bool invulnerable;
    
    bool isDead = false;
    const bool DEBUG = true;

    private bool isItPaused = false;

    // Almacena las frases para el sistema de diálogo
    private string[] frases = { "Texto por defecto, cambia el index", "Hola mi nombre es Ben, bienvenido!", "Seguramente necesite esto", "Mi escudo esta a punto de romperse!", "Necesito curar mis heridas" };

    // Definir como único GameManager y designar quién será la UIManager
    UIManager UIManager;

    #region Singleton
    public static GameManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }
    #endregion

    private void Start()
    {
        Time.timeScale = 1f;
        invulnerable = false;
        UIManager = UIManager.instance;
        UIManager.PauseMenu(isItPaused);
        playerHP = playerMaxHP;
        shieldHP = shieldMaxHP;
        shieldWeight = 0;
        
        UIManager.UpdateHealthBar(playerMaxHP, playerHP);
        UIManager.UpdateShieldBar(shieldHP, shieldMaxHP);
        UIManager.UpdateShieldHolder();
        // Dar valores a lastCheckpoint y a checkpointShield
    }

    private void Update()
    {
        if (Input.GetButtonDown("Escape")) 
        {
            if (isItPaused) 
            {
                isItPaused = false;
                UIManager.PauseMenu(isItPaused);
                Time.timeScale = 1f;                
            }

            else 
            {
                isItPaused = true;
                UIManager.PauseMenu(isItPaused);                
                Time.timeScale = 0f;
            }        
        }
    }

    public void SetPlayer(GameObject p)
    {
        player = p;
        shield = p.transform.GetChild(0).GetChild(0).GetChild(0).gameObject;
    }


    public void GetShield(float healPoints, float weight, Sprite newsprite) // Inicia los valores al coger un escudo
    {
        //Actualizamos los valores de peso y salud del escudo
        shieldMaxHP = healPoints;
        shieldHP = healPoints;
        shieldWeight = weight;
        shield.GetComponent<SpriteRenderer>().sprite = newsprite;
        UIManager.UpdateShieldBar(healPoints, healPoints);
    }

    public void OnHit(GameObject obj, float damage) // Quita vida al jugador (colisión con enemigo)
    {
        if (!invulnerable)
        {
            if (obj.tag == "Shield")
            {
                shieldHP -= damage;
                // Llamar al UIManager
                UIManager.UpdateShieldBar(shieldMaxHP, shieldHP);

                if (shieldHP < 0)
                {
                    playerHP += shieldHP; // Si el escudo queda con vida negativa, hace también daño al jugador
                                          // Llamar al UIManager

                    UIManager.UpdateHealthBar(playerMaxHP, playerHP);
                }
                if (damage > 10)
                {
                    invulnerable = true;
                    Invoke("InvulnerableTimer", 0.2f);
                }
            }
            else if (obj.tag == "Player")
            {
                playerHP -= damage;
                // Llamar al UIManager
                UIManager.UpdateHealthBar(playerMaxHP, playerHP);
                if (damage > 10)
                {
                    invulnerable = true;
                    Invoke("InvulnerableTimer", 0.2f);
                }
            }
        }
        else if (obj.tag == "Player")
        {
            playerHP -= damage;
            // Llamar al UIManager
            UIManager.UpdateHealthBar(playerMaxHP, playerHP);
        }

        if (playerHP <= 0) OnDead(player);
    }
    private void InvulnerableTimer()
    {
        invulnerable = false;
    }

    public void OnHeal(float heal) // Cura al jugador (colision con botiquines)
    {
        Debug.Log("Heal + " + heal);

        if (playerHP + heal > playerMaxHP)
        {
            playerHP = playerMaxHP;
        }

        else
        {
            playerHP += heal;
        }

        UIManager.UpdateHealthBar(playerMaxHP, playerHP);
    }

    public void OnRepair(float reapairvalue) // Cura al jugador (colision con botiquines)
    {
        Debug.Log("Repair + " + reapairvalue);

        if (shieldHP + reapairvalue > shieldMaxHP)
        {
            shieldHP = shieldMaxHP;
        }

        else
        {
            shieldHP += reapairvalue;
        }

        UIManager.UpdateShieldBar(shieldMaxHP, shieldHP);
    }

    public void Checkpoint(Vector2 pos, Sprite s) // Guarda los valores al pasar por un checkpoint 
    {
        lastCheckpoint = pos;
        checkpointPlayerHP = playerHP;
        checkpointShieldHP = shieldHP;
        checkpointShieldWeight = shieldWeight;
        checkpointShield = s;
    }

    public void OnDead(GameObject player) // Resetea desde el checkpoint
    {
        // Llamar a un método del jugador para que cambie a la posición de 
        //lastCheckpoint (enviada como parámetro) y le envíe el sprite del escudo
        Checkpoint(lastCheckpoint, checkpointShield);
        player.transform.position = lastCheckpoint;
    }

    public void OnDialogue(int index)
    {
        Debug.Log("OnDialogue index " + index);
        UIManager.Dialogue(frases[index]);
    }

    public void MainMenu() 
    {
        SceneManager.LoadScene("00_MainMenu");
    }
    public void Exit() 
    {
        Debug.Log("El juego se ha cerrado");
        Application.Quit();
    }
}
