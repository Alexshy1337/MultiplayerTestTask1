using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputValidationUtils
{

    private static string validCharacters = "abcdefghijklmnopqrstuvxywzABCDEFGHIJKLMNOPQRSTUVXYWZ-_0123456789";

    public static char ValidateChar(string validCharacters, char addedChar)
    {
        if (validCharacters.IndexOf(addedChar) != -1)
        {
            // Valid
            return addedChar;
        }
        else
        {
            // Invalid
            return '\0';
        }
    }

    public static char onValidate(string text, int charIndex, char addedChar){
            return ValidateChar(validCharacters, addedChar);
    }

}
