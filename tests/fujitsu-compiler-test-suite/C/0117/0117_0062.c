#include <stdio.h>
struct st {
  float a1,a2,a3,a4,a5,a6,b1,b2,b3,b4,b5,b6;
};

void set_value(struct st *st1)
{
  st1->a1=st1->a2=st1->a3=st1->a4=st1->a5=st1->a6=1.0;
  st1->b1=st1->b2=st1->b3=st1->b4=st1->b5=st1->b6=0.0;
}
int main()
{
  struct st st1;
  float a,b,c;
  int i;
  int judge[10]={0,1,2,3,4,5,6,7,8,9};
  set_value(&st1);

  


  a=b=c=0.0; i=0;

  do{
    switch(judge[i]){
    case 1:
      a=(st1.a1 && !st1.b1)?(!st1.b2 && !st1.b3)?1:2:3;   break;
    case 2:
      b=((st1.a2 || st1.b1) && st1.a3)?(st1.b4 || st1.b5 || !st1.b6)?1:2:3;   break;
    case 3:
      c=(!st1.b3)?(!st1.b4)?1:2:3;   break;
    }
    i++;
  }  while(i<10);

  if(a==1.0 && b==1.0 && c==1.0)
    printf("ok\n");
  else{
    printf("ng\n");
    printf("a=%Lf b=%Lf c=%Lf\n",a,b,c);
  }
}
