using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class SceneFader : MonoBehaviour
{

    public Image fadeImage;

    public float fadeSpeed = 2f;



    public IEnumerator FadeOut()
    {

        float alpha = 0;


        while (alpha < 1)
        {

            alpha += Time.deltaTime * fadeSpeed;


            Color c = fadeImage.color;
            c.a = alpha;
            fadeImage.color = c;


            yield return null;

        }

    }



    public IEnumerator FadeIn()
    {

        float alpha = 1;


        while (alpha > 0)
        {

            alpha -= Time.deltaTime * fadeSpeed;


            Color c = fadeImage.color;
            c.a = alpha;
            fadeImage.color = c;


            yield return null;

        }

    }

}