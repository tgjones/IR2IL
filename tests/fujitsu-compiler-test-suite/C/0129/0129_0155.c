#include <stdlib.h>
#include <string.h>
#include <math.h>
#include  <stdio.h>
int func();
struct homerun
{
   int some[3];
   int hit;
   int str;
   int sum;
};
  struct homerun aaa[50];
  struct homerun *bbb;
  int n,i,j,hs,h,c,demo,d,e;
  static int ccc[3]={7,2,5};
  int table[3];
  int strnum[3];
  int hitnum[3];
int main()
{
  d=1;
  bbb=aaa;
  i=0;
  c=0;
  hs=0;
  n=0;
  while(i<=2){
    if(hs==3){
      break;
    }
    table[0]=d;
    table[1]=d+1;
    table[2]=d+2;
    func();
    i++;
    d=d+3;
  }
  if(hs!=3){
    table[c]=0;
    c++;
  }
  while(h<=2){
    switch(c){
      case 0:break;
      case 1:
               switch(n){
               case 1:table[2]=strnum[0];
                        table[0]=hitnum[0];
                        table[1]=hitnum[1];
                        break;
               case 2:table[1]=strnum[0];
                        table[0]=hitnum[0];
                        table[2]=hitnum[2];
                        break;
               case 3:table[0]=strnum[0];
                        table[1]=hitnum[1];
                        table[2]=hitnum[2];
                        break;
               }
      break;
      case 2:
               switch(n){
               case 0:table[2]=strnum[1];
                        table[1]=strnum[2];
                        table[0]=hitnum[0];
                        break;
               case 1:table[0]=strnum[2];
                        table[2]=strnum[0];
                        table[1]=hitnum[1];
                        break;
               case 2:table[0]=strnum[1];
                        table[1]=strnum[0];
                        table[2]=hitnum[2];
                        break;
               }
      break;
      case 3:table[1]=strnum[0];
               table[2]=strnum[1];
               table[0]=strnum[2];
      }
      n=0;
      c=0;
      hs=0;
      func();
  }
  if(memcmp(ccc,hitnum,12)==0)
    printf("***** ok *****\n");
  else
    printf("***** ng *****\n");
exit (0);
}
int func()
{
  h=0;
  bbb->some[0]=table[0];
  bbb->some[1]=table[1];
  bbb->some[2]=table[2];
  bbb->hit=0;
  bbb->str=0;
  bbb->sum=0;
  e=0;
  while(e<=2){
    if(ccc[e]==bbb->some[e]) {
      bbb->hit++;
      hitnum[e]=bbb->some[e];
      n=n+e;
      h++;
    }
    j=0;
    while(j<=2){
      if(ccc[e]==bbb->some[j] && e!=j) {
        bbb->str++;
        strnum[c]=bbb->some[j];
        c++;
      }
      j++;
    }
    e++;
  }
  bbb->sum=bbb->hit+bbb->str;
  hs=hs+bbb->sum;
  bbb++;
}
