﻿using System.Collections;
using UnityEngine;

// Este script se usa como medio general de comunicación, principalmente para avisar a la UI o para actualizar el estado del jugador.
public class GameManager : MonoBehaviour
{
    const float playerMaxHP = 200f;
    float shieldMaxHP = 60f;
    float playerHP, shieldHP;
    GameObject shield;

    AudioManager audioManager;
    PlayerController playerController;

    bool hasKey = false;

    [SerializeField] UIManager UIManager;
    [SerializeField] GameObject player;
    [SerializeField] Sound playerHit;
    [SerializeField] Shield[] shieldArray;

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
    }
    #endregion

    private void Start()
    {
        audioManager = AudioManager.instance;

        // Recogemos cierta información del player para usar durante la ejecución.
        shield = player.transform.GetChild(0).GetChild(0).GetChild(0).gameObject;
        playerController = player.GetComponent<PlayerController>();        
        playerHP = playerMaxHP;
        shieldHP = shieldMaxHP;
        GetShield(SceneLoader.instance.CheckShield());

        // Actualizamos la UI
        if (UIManager != null)
        {
            UIManager.UpdateHealthBar(playerMaxHP, playerHP);
            UIManager.UpdateShieldBar(shieldHP, shieldMaxHP);
        }
    }

    // Al cargar la escena spawnea al player en el punto adecuado.
    public void SpawnPlayer(Vector2 point)
    {
        playerHP = playerMaxHP;
        shieldHP = shieldMaxHP;
        player.transform.position = point;
        Debug.Log("El jugador ha spawneado en " + point + " con " + playerHP + " HP y " + shieldHP + " de escudo.");
    }

    // Al recoger un escudo actualiza los valores adecuados.
    public void GetShield(ShieldType sType)
    {
        SceneLoader.instance.SetShield(sType);

        int i = 0;
        //Buscamos el escudo recogido entre el array de escudos conocidos
        while (i < shieldArray.Length && shieldArray[i].shieldType != sType)
            i++;
        //Si no se encontró, no hacemos nada
        if (i == shieldArray.Length)
            return;

        //Actualizamos los PS del escudo, así como sus PS máximos
        shieldHP = shieldMaxHP = shieldArray[i].durability;
        playerController.ShieldBroken(false);

        //Actualizamos la velocidad del jugador según el peso del escudo nuevo        
        if (playerController != null)
            playerController.AddWeight(shieldArray[i].weight);

        //Actualizamos el sprite y el icono del escudo
        UIManager.UpdateShieldHolder(shieldArray[i].sprite);
        UIManager.UpdateShieldBar(shieldMaxHP, shieldHP);
        shield.GetComponent<SpriteRenderer>().sprite = shieldArray[i].sprite;
    }

    // Cuando el jugador o el escudo reciben daño se llama a este método
    public void OnHit(GameObject obj, float damage)
    {
        if (obj.GetComponent<ShieldClass>() != null)
        {
            shieldHP -= damage;
            if (shieldHP <= 0)
            {
                playerController.ShieldBroken(true);
                playerHP += shieldHP;
                audioManager.PlaySoundOnce(playerHit);
            }
        }

        else if (obj.GetComponent<PlayerController>() != null)
        {
            UIManager.DamageOverlay();
            audioManager.PlaySoundOnce(playerHit);
            playerHP -= damage;

            if (playerHP < 0)
            {
                StartCoroutine(OnDead());
            }
        }

        UIManager.UpdateShieldBar(shieldMaxHP, shieldHP);
        UIManager.UpdateHealthBar(playerMaxHP, playerHP);
    }

    // Incia el sistema de respawn con un poco de delay.
    IEnumerator OnDead()
    {
        Destroy(player);
        yield return new WaitForSeconds(2f);
        SceneLoader.instance.ResetScene();
    }

    // Cura al jugador
    public void OnHeal(float heal) 
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

    // Repara el escudo
    public void OnRepair(float reapairvalue)
    {
        Debug.Log("Repair + " + reapairvalue);
        playerController.ShieldBroken(false);

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

    // Le dice al UIManager que frase debe mostrar en pantalla
    public void OnDialogue(string frase)
    {
        UIManager.OnDialogue(frase);
    }

    // Comprueba si el jugador tiene una llave disponible
    public bool HasKey()
    {
        if (hasKey)
        {
            hasKey = false;
            return true;
        }
        else
            return false;
    }

    // Actualiza el estado de la llave al recogerla
    public void KeyPickup()
    {        
        hasKey = true;
    }
}
