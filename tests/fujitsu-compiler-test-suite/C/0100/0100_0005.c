#include <stdio.h>

#define ROTATION_NUM 100
#define ELEMENT_NUM 100
#define ANSWER -9200.0f

void sub_005(int (*aa)[ELEMENT_NUM],
	      int (*bb)[ELEMENT_NUM], 
	      int pi,
	      int pj,
	      int pk,
	      int pl,
	      int pm,
	      int pn)
{
  int i,j;
    for (j=pi/5+pm+pn; j < ROTATION_NUM+pj; ++j) {
      for (i=10/2; i < ROTATION_NUM+pi+pl-pm+pn/3-2-1+pk+pi+pj; i=i+1+pj*pn) {
	aa[i][j] = aa[i][j] - bb[i][j];
    }
  }
}


int main( void ) {
  int array2_a[ELEMENT_NUM][ELEMENT_NUM];
  int array2_b[ELEMENT_NUM][ELEMENT_NUM];
  double result=0;
  int i,j,k,l,m,n;
  i=j=k=l=m=n=0;
  
  
  printf("%s \n","initialization");
  for (i=0; i < ROTATION_NUM; ++i) {
    for (j=0; j < ROTATION_NUM; ++j) {
      array2_a[i][j] = 0;
      array2_b[i][j] = 1;
    }
  }
  
  
  i=j=0;
  printf("%s \n","Main body of processing");
  sub_005(array2_a, array2_b, i, j, k, l, m, n);
  
  
  
  printf("%s \n","check on result");
  for (i=0; i < ROTATION_NUM; ++i) {
    for (j=0; j < ROTATION_NUM; ++j) {
      result += array2_a[i][j];
    }
  }
  
  if( (result - ANSWER) == 0 ) {
    printf("%s\n", "ok");
  } else {
    printf("%s: \n", "ng");
    printf("Correct Answer: %lf \n", ANSWER);
    printf("Execution Result: %lf \n", result);
  }
  return 1;
}
