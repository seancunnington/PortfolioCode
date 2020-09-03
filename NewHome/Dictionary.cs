using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
public class Dictionary : MonoBehaviour {

     
     [Header("Add Word")]
     public string wordToAdd;
     public bool addWord = false;


     [Header("Delete Word")]
     public string wordToDelete;
     public bool deleteWord = false;


     [Header("Set To Uppercase")]
     public bool set;

     [Header("Dictionary")]
     public bool sort = false;
     public List<string> dictionary = new List<string>();


     private void Update()
     {
          if (sort) SortDictionary();
          if (addWord) AddWord(wordToAdd);
          if (deleteWord) DeleteWord(wordToDelete);
          if (set) SetDictionaryToUpper();
     }



     // Set the entire dictionary to Upper Case.
     public void SetDictionaryToUpper()
     {
          for (int i = 0; i < dictionary.Count; i++)
          {
               dictionary[i] = dictionary[i].ToUpper();
          }

          set = false;
          Debug.Log("Dicitonary set to Uppercase.");
     }



     private void SortDictionary()
     {
          dictionary.Sort();
          Debug.Log("Dictionary Sorted");

          // Stop sorting
          sort = false;
     }


     // Add the word to the dictionary if it is not a copy.
     public void AddWord(string newWord)
     {
          string word = newWord.ToUpper();

          if (dictionary.Contains(word))
          {
               Debug.Log("Word already exists in this dictionary.");
          }
          else
          {
               dictionary.Add(word);
               dictionary.Sort();
               Debug.Log("Word added and dictionary sorted.");
               wordToAdd = "";
          }
          addWord = false;
     }


     // Delete the word from the list and shorten list by one.
     private void DeleteWord (string oldWord)
     {
          string word = oldWord.ToUpper();

          if (dictionary.Contains(word))
          {
               dictionary.Remove(word);
               Debug.Log("Word found and deleted.");
               wordToDelete = "";
               deleteWord = false;
          }
          else
          {
               Debug.Log("Word to delete not found.");
          }
     }


     
}
