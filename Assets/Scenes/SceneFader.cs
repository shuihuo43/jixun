using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class SceneFader : MonoBehaviour
{

    public Image fadeImage;


    public IEnumerator FadeOut()
    {

        float t = 0;


        while (t < 1)
        {

            t += Time.deltaTime;


            Color c = fadeImage.color;

            c.a = t;

            fadeImage.color = c;


            yield return null;

        }

    }

}