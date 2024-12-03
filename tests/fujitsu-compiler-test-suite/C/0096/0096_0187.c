
#include <stdio.h>

#define LOOP_CONST 256
#define TRUE_RES 1
#define FALSE_RES 0
#define IF_RES 1
#define ELSEIF1_RES 2
#define ELSEIF2_RES 3

#define INITIALIZE_METHOD(init_type) \
void initialize_##init_type(init_type *data, int num) {\
  int i;\
  for(i=0;i<num;i++) {\
    data[i] = (init_type)FALSE_RES;\
  }\
}

struct fstruct {\
  int flag1;\
  int flag2;\
  int flag3;\
  int flag4;\
  int flag5;\
}flag;

#define TEST_LOOP_NO_ESCAPE_PT1(test_type) \
int test_loop_no_escape_pt1_##test_type(struct fstruct flag, int lmax, test_type d_val1, test_type d_val2) {\
  int i;\
  test_type d_data1[LOOP_CONST],d_data2[LOOP_CONST],d_data3[LOOP_CONST],d_data4[LOOP_CONST];\
  initialize_##test_type(d_data1, LOOP_CONST);\
  initialize_##test_type(d_data2, LOOP_CONST);\
  initialize_##test_type(d_data3, LOOP_CONST);\
  initialize_##test_type(d_data4, LOOP_CONST);\
  for(i=0;i<LOOP_CONST;i++) {\
    if ((test_type)flag.flag5 <= i) {\
      if ((test_type)flag.flag1 < (test_type)flag.flag2) {\
        d_data1[i] = (test_type)IF_RES;\
      } else if ((test_type)flag.flag1 > (test_type)flag.flag2) {\
        d_data1[i] = (test_type)ELSEIF1_RES;\
      } else if (!(test_type)flag.flag1) {\
        d_data1[i] = (test_type)ELSEIF2_RES;\
      }\
      d_data3[i] = (test_type)IF_RES;\
      if ((test_type)flag.flag3 < (test_type)flag.flag4) {\
        d_data2[i] = (test_type)IF_RES;\
      } else if ((test_type)flag.flag3 > (test_type)flag.flag4) {\
        d_data2[i] = (test_type)ELSEIF1_RES;\
      } else if (!(test_type)flag.flag3) {\
        d_data2[i] = (test_type)ELSEIF2_RES;\
      }\
      d_data4[i] = (test_type)IF_RES;\
    }\
  }\
  for(i=0;i<LOOP_CONST;i++) {\
    if (d_data1[i] != d_val1 || d_data2[i] != d_val2) {\
      return FALSE_RES;\
    }\
    if (d_data3[i] != (test_type)IF_RES) {\
      return FALSE_RES;\
    }\
    if (d_data4[i] != (test_type)IF_RES) {\
      return FALSE_RES;\
    }\
  }\
  return TRUE_RES;\
}

#define TEST_LOOP_NO_ESCAPE_PT2(test_type) \
int test_loop_no_escape_pt2_##test_type(struct fstruct flag, int lmax, test_type d_val1, test_type d_val2) {\
  int i;\
  test_type d_data1[LOOP_CONST],d_data2[LOOP_CONST],d_data3[LOOP_CONST],d_data4[LOOP_CONST];\
  initialize_##test_type(d_data1, LOOP_CONST);\
  initialize_##test_type(d_data2, LOOP_CONST);\
  initialize_##test_type(d_data3, LOOP_CONST);\
  initialize_##test_type(d_data4, LOOP_CONST);\
  for(i=0;i<lmax;i++) {\
    if ((test_type)flag.flag5 <= i) {\
      if ((test_type)flag.flag1 < (test_type)flag.flag2) {\
        d_data1[i] = (test_type)IF_RES;\
      } else if ((test_type)flag.flag1 > (test_type)flag.flag2) {\
        d_data1[i] = (test_type)ELSEIF1_RES;\
      } else if (!(test_type)flag.flag1) {\
        d_data1[i] = (test_type)ELSEIF2_RES;\
      }\
      d_data3[i] = (test_type)IF_RES;\
      if ((test_type)flag.flag3 < (test_type)flag.flag4) {\
        d_data2[i] = (test_type)IF_RES;\
      } else if ((test_type)flag.flag3 > (test_type)flag.flag4) {\
        d_data2[i] = (test_type)ELSEIF1_RES;\
      } else if (!(test_type)flag.flag3) {\
        d_data2[i] = (test_type)ELSEIF2_RES;\
      }\
      d_data4[i] = (test_type)IF_RES;\
    }\
  }\
  for(i=0;i<LOOP_CONST;i++) {\
    if (lmax > 0) {\
      if (d_data1[i] != d_val1 || d_data2[i] != d_val2) {\
        return FALSE_RES;\
      }\
      if (d_data3[i] != (test_type)IF_RES) {\
        return FALSE_RES;\
      }\
      if (d_data4[i] != (test_type)IF_RES) {\
        return FALSE_RES;\
      }\
    } else {\
      if (d_data1[i] != (test_type)FALSE_RES || d_data2[i] != (test_type)FALSE_RES || d_data3[i] != (test_type)FALSE_RES || d_data4[i] != (test_type)FALSE_RES) {\
        return FALSE_RES;\
      }\
    }\
  }\
  return TRUE_RES;\
}

#define CALL_TEST_LOOP_NO_ESCAPE(test_type,flag_value1, flag_value2, flag_value3, flag_value4, flag_value5, loop_max, comp_value1, comp_value2) \
flag.flag1 = flag_value1;  flag.flag2 = flag_value2;  flag.flag3 = flag_value3;  flag.flag4 = flag_value4;  flag.flag5 = flag_value5; \
if (!test_loop_no_escape_pt1_##test_type(flag,loop_max,(test_type)comp_value1,(test_type)comp_value2) ||\
    !test_loop_no_escape_pt2_##test_type(flag,loop_max,(test_type)comp_value1,(test_type)comp_value2)) puts("NG : "#test_type);

INITIALIZE_METHOD(float);
TEST_LOOP_NO_ESCAPE_PT1(float);
TEST_LOOP_NO_ESCAPE_PT2(float);

INITIALIZE_METHOD(double);
TEST_LOOP_NO_ESCAPE_PT1(double);
TEST_LOOP_NO_ESCAPE_PT2(double);

typedef long double LD;
INITIALIZE_METHOD(LD);
TEST_LOOP_NO_ESCAPE_PT1(LD);
TEST_LOOP_NO_ESCAPE_PT2(LD);

INITIALIZE_METHOD(char);
TEST_LOOP_NO_ESCAPE_PT1(char);
TEST_LOOP_NO_ESCAPE_PT2(char);

typedef short int SI;
INITIALIZE_METHOD(SI);
TEST_LOOP_NO_ESCAPE_PT1(SI);
TEST_LOOP_NO_ESCAPE_PT2(SI);

INITIALIZE_METHOD(int);
TEST_LOOP_NO_ESCAPE_PT1(int);
TEST_LOOP_NO_ESCAPE_PT2(int);

typedef long int LI;
INITIALIZE_METHOD(LI);
TEST_LOOP_NO_ESCAPE_PT1(LI);
TEST_LOOP_NO_ESCAPE_PT2(LI);

typedef long long int LLI;
INITIALIZE_METHOD(LLI);
TEST_LOOP_NO_ESCAPE_PT1(LLI);
TEST_LOOP_NO_ESCAPE_PT2(LLI);

typedef unsigned char UC;
INITIALIZE_METHOD(UC);
TEST_LOOP_NO_ESCAPE_PT1(UC);
TEST_LOOP_NO_ESCAPE_PT2(UC);

typedef unsigned char US;
INITIALIZE_METHOD(US);
TEST_LOOP_NO_ESCAPE_PT1(US);
TEST_LOOP_NO_ESCAPE_PT2(US);

typedef unsigned int UI;
INITIALIZE_METHOD(UI);
TEST_LOOP_NO_ESCAPE_PT1(UI);
TEST_LOOP_NO_ESCAPE_PT2(UI);

typedef unsigned long int UL;
INITIALIZE_METHOD(UL);
TEST_LOOP_NO_ESCAPE_PT1(UL);
TEST_LOOP_NO_ESCAPE_PT2(UL);

typedef unsigned long long int ULL;
INITIALIZE_METHOD(ULL);
TEST_LOOP_NO_ESCAPE_PT1(ULL);
TEST_LOOP_NO_ESCAPE_PT2(ULL);

int main() {
  CALL_TEST_LOOP_NO_ESCAPE(float, 64, 96, 64, 96, 0, LOOP_CONST, IF_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(float, 64, 96, 96, 64, 0, LOOP_CONST, IF_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(float, 64, 96, 0, 0, 0, LOOP_CONST, IF_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(float, 64, 96, 64, 64, 0, LOOP_CONST, IF_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(float, 64, 32, 96, 64, 0, LOOP_CONST, ELSEIF1_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(float, 64, 32, 64, 96, 0, LOOP_CONST, ELSEIF1_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(float, 64, 32, 0, 0, 0, LOOP_CONST, ELSEIF1_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(float, 64, 32, 64, 64, 0, LOOP_CONST, ELSEIF1_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(float, 0, 0, 0, 0, 0, LOOP_CONST, ELSEIF2_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(float, 0, 0, 64, 96, 0, LOOP_CONST, ELSEIF2_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(float, 0, 0, 96, 64, 0, LOOP_CONST, ELSEIF2_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(float, 0, 0, 64, 64, 0, LOOP_CONST, ELSEIF2_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(float, 64, 64, 64, 96, 0, LOOP_CONST, FALSE_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(float, 64, 64, 64, 32, 0, LOOP_CONST, FALSE_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(float, 64, 64, 0, 0, 0, LOOP_CONST, FALSE_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(float, 64, 64, 64, 64, 0, LOOP_CONST, FALSE_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(double, 64, 96, 64, 96, 0, LOOP_CONST, IF_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(double, 64, 96, 96, 64, 0, LOOP_CONST, IF_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(double, 64, 96, 0, 0, 0, LOOP_CONST, IF_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(double, 64, 96, 64, 64, 0, LOOP_CONST, IF_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(double, 64, 32, 96, 64, 0, LOOP_CONST, ELSEIF1_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(double, 64, 32, 64, 96, 0, LOOP_CONST, ELSEIF1_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(double, 64, 32, 0, 0, 0, LOOP_CONST, ELSEIF1_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(double, 64, 32, 64, 64, 0, LOOP_CONST, ELSEIF1_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(double, 0, 0, 0, 0, 0, LOOP_CONST, ELSEIF2_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(double, 0, 0, 64, 96, 0, LOOP_CONST, ELSEIF2_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(double, 0, 0, 96, 64, 0, LOOP_CONST, ELSEIF2_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(double, 0, 0, 64, 64, 0, LOOP_CONST, ELSEIF2_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(double, 64, 64, 64, 96, 0, LOOP_CONST, FALSE_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(double, 64, 64, 64, 32, 0, LOOP_CONST, FALSE_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(double, 64, 64, 0, 0, 0, LOOP_CONST, FALSE_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(double, 64, 64, 64, 64, 0, LOOP_CONST, FALSE_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LD, 64, 96, 64, 96, 0, LOOP_CONST, IF_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LD, 64, 96, 96, 64, 0, LOOP_CONST, IF_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LD, 64, 96, 0, 0, 0, LOOP_CONST, IF_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LD, 64, 96, 64, 64, 0, LOOP_CONST, IF_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LD, 64, 32, 96, 64, 0, LOOP_CONST, ELSEIF1_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LD, 64, 32, 64, 96, 0, LOOP_CONST, ELSEIF1_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LD, 64, 32, 0, 0, 0, LOOP_CONST, ELSEIF1_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LD, 64, 32, 64, 64, 0, LOOP_CONST, ELSEIF1_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LD, 0, 0, 0, 0, 0, LOOP_CONST, ELSEIF2_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LD, 0, 0, 64, 96, 0, LOOP_CONST, ELSEIF2_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LD, 0, 0, 96, 64, 0, LOOP_CONST, ELSEIF2_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LD, 0, 0, 64, 64, 0, LOOP_CONST, ELSEIF2_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LD, 64, 64, 64, 96, 0, LOOP_CONST, FALSE_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LD, 64, 64, 64, 32, 0, LOOP_CONST, FALSE_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LD, 64, 64, 0, 0, 0, LOOP_CONST, FALSE_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LD, 64, 64, 64, 64, 0, LOOP_CONST, FALSE_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(char, 64, 96, 64, 96, 0, LOOP_CONST, IF_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(char, 64, 96, 96, 64, 0, LOOP_CONST, IF_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(char, 64, 96, 0, 0, 0, LOOP_CONST, IF_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(char, 64, 96, 64, 64, 0, LOOP_CONST, IF_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(char, 64, 32, 96, 64, 0, LOOP_CONST, ELSEIF1_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(char, 64, 32, 64, 96, 0, LOOP_CONST, ELSEIF1_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(char, 64, 32, 0, 0, 0, LOOP_CONST, ELSEIF1_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(char, 64, 32, 64, 64, 0, LOOP_CONST, ELSEIF1_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(char, 0, 0, 0, 0, 0, LOOP_CONST, ELSEIF2_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(char, 0, 0, 64, 96, 0, LOOP_CONST, ELSEIF2_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(char, 0, 0, 96, 64, 0, LOOP_CONST, ELSEIF2_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(char, 0, 0, 64, 64, 0, LOOP_CONST, ELSEIF2_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(char, 64, 64, 64, 96, 0, LOOP_CONST, FALSE_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(char, 64, 64, 64, 32, 0, LOOP_CONST, FALSE_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(char, 64, 64, 0, 0, 0, LOOP_CONST, FALSE_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(char, 64, 64, 64, 64, 0, LOOP_CONST, FALSE_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(SI, 64, 96, 64, 96, 0, LOOP_CONST, IF_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(SI, 64, 96, 96, 64, 0, LOOP_CONST, IF_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(SI, 64, 96, 0, 0, 0, LOOP_CONST, IF_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(SI, 64, 96, 64, 64, 0, LOOP_CONST, IF_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(SI, 64, 32, 96, 64, 0, LOOP_CONST, ELSEIF1_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(SI, 64, 32, 64, 96, 0, LOOP_CONST, ELSEIF1_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(SI, 64, 32, 0, 0, 0, LOOP_CONST, ELSEIF1_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(SI, 64, 32, 64, 64, 0, LOOP_CONST, ELSEIF1_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(SI, 0, 0, 0, 0, 0, LOOP_CONST, ELSEIF2_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(SI, 0, 0, 64, 96, 0, LOOP_CONST, ELSEIF2_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(SI, 0, 0, 96, 64, 0, LOOP_CONST, ELSEIF2_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(SI, 0, 0, 64, 64, 0, LOOP_CONST, ELSEIF2_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(SI, 64, 64, 64, 96, 0, LOOP_CONST, FALSE_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(SI, 64, 64, 64, 32, 0, LOOP_CONST, FALSE_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(SI, 64, 64, 0, 0, 0, LOOP_CONST, FALSE_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(SI, 64, 64, 64, 64, 0, LOOP_CONST, FALSE_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(int, 64, 96, 64, 96, 0, LOOP_CONST, IF_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(int, 64, 96, 96, 64, 0, LOOP_CONST, IF_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(int, 64, 96, 0, 0, 0, LOOP_CONST, IF_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(int, 64, 96, 64, 64, 0, LOOP_CONST, IF_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(int, 64, 32, 96, 64, 0, LOOP_CONST, ELSEIF1_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(int, 64, 32, 64, 96, 0, LOOP_CONST, ELSEIF1_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(int, 64, 32, 0, 0, 0, LOOP_CONST, ELSEIF1_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(int, 64, 32, 64, 64, 0, LOOP_CONST, ELSEIF1_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(int, 0, 0, 0, 0, 0, LOOP_CONST, ELSEIF2_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(int, 0, 0, 64, 96, 0, LOOP_CONST, ELSEIF2_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(int, 0, 0, 96, 64, 0, LOOP_CONST, ELSEIF2_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(int, 0, 0, 64, 64, 0, LOOP_CONST, ELSEIF2_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(int, 64, 64, 64, 96, 0, LOOP_CONST, FALSE_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(int, 64, 64, 64, 32, 0, LOOP_CONST, FALSE_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(int, 64, 64, 0, 0, 0, LOOP_CONST, FALSE_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(int, 64, 64, 64, 64, 0, LOOP_CONST, FALSE_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LI, 64, 96, 64, 96, 0, LOOP_CONST, IF_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LI, 64, 96, 96, 64, 0, LOOP_CONST, IF_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LI, 64, 96, 0, 0, 0, LOOP_CONST, IF_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LI, 64, 96, 64, 64, 0, LOOP_CONST, IF_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LI, 64, 32, 96, 64, 0, LOOP_CONST, ELSEIF1_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LI, 64, 32, 64, 96, 0, LOOP_CONST, ELSEIF1_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LI, 64, 32, 0, 0, 0, LOOP_CONST, ELSEIF1_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LI, 64, 32, 64, 64, 0, LOOP_CONST, ELSEIF1_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LI, 0, 0, 0, 0, 0, LOOP_CONST, ELSEIF2_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LI, 0, 0, 64, 96, 0, LOOP_CONST, ELSEIF2_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LI, 0, 0, 96, 64, 0, LOOP_CONST, ELSEIF2_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LI, 0, 0, 64, 64, 0, LOOP_CONST, ELSEIF2_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LI, 64, 64, 64, 96, 0, LOOP_CONST, FALSE_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LI, 64, 64, 64, 32, 0, LOOP_CONST, FALSE_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LI, 64, 64, 0, 0, 0, LOOP_CONST, FALSE_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LI, 64, 64, 64, 64, 0, LOOP_CONST, FALSE_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LLI, 64, 96, 64, 96, 0, LOOP_CONST, IF_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LLI, 64, 96, 96, 64, 0, LOOP_CONST, IF_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LLI, 64, 96, 0, 0, 0, LOOP_CONST, IF_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LLI, 64, 96, 64, 64, 0, LOOP_CONST, IF_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LLI, 64, 32, 96, 64, 0, LOOP_CONST, ELSEIF1_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LLI, 64, 32, 64, 96, 0, LOOP_CONST, ELSEIF1_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LLI, 64, 32, 0, 0, 0, LOOP_CONST, ELSEIF1_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LLI, 64, 32, 64, 64, 0, LOOP_CONST, ELSEIF1_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LLI, 0, 0, 0, 0, 0, LOOP_CONST, ELSEIF2_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LLI, 0, 0, 64, 96, 0, LOOP_CONST, ELSEIF2_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LLI, 0, 0, 96, 64, 0, LOOP_CONST, ELSEIF2_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LLI, 0, 0, 64, 64, 0, LOOP_CONST, ELSEIF2_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LLI, 64, 64, 64, 96, 0, LOOP_CONST, FALSE_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LLI, 64, 64, 64, 32, 0, LOOP_CONST, FALSE_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LLI, 64, 64, 0, 0, 0, LOOP_CONST, FALSE_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LLI, 64, 64, 64, 64, 0, LOOP_CONST, FALSE_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UC, 64, 96, 64, 96, 0, LOOP_CONST, IF_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UC, 64, 96, 96, 64, 0, LOOP_CONST, IF_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UC, 64, 96, 0, 0, 0, LOOP_CONST, IF_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UC, 64, 96, 64, 64, 0, LOOP_CONST, IF_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UC, 64, 32, 96, 64, 0, LOOP_CONST, ELSEIF1_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UC, 64, 32, 64, 96, 0, LOOP_CONST, ELSEIF1_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UC, 64, 32, 0, 0, 0, LOOP_CONST, ELSEIF1_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UC, 64, 32, 64, 64, 0, LOOP_CONST, ELSEIF1_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UC, 0, 0, 0, 0, 0, LOOP_CONST, ELSEIF2_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UC, 0, 0, 64, 96, 0, LOOP_CONST, ELSEIF2_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UC, 0, 0, 96, 64, 0, LOOP_CONST, ELSEIF2_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UC, 0, 0, 64, 64, 0, LOOP_CONST, ELSEIF2_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UC, 64, 64, 64, 96, 0, LOOP_CONST, FALSE_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UC, 64, 64, 64, 32, 0, LOOP_CONST, FALSE_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UC, 64, 64, 0, 0, 0, LOOP_CONST, FALSE_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UC, 64, 64, 64, 64, 0, LOOP_CONST, FALSE_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(US, 64, 96, 64, 96, 0, LOOP_CONST, IF_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(US, 64, 96, 96, 64, 0, LOOP_CONST, IF_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(US, 64, 96, 0, 0, 0, LOOP_CONST, IF_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(US, 64, 96, 64, 64, 0, LOOP_CONST, IF_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(US, 64, 32, 96, 64, 0, LOOP_CONST, ELSEIF1_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(US, 64, 32, 64, 96, 0, LOOP_CONST, ELSEIF1_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(US, 64, 32, 0, 0, 0, LOOP_CONST, ELSEIF1_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(US, 64, 32, 64, 64, 0, LOOP_CONST, ELSEIF1_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(US, 0, 0, 0, 0, 0, LOOP_CONST, ELSEIF2_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(US, 0, 0, 64, 96, 0, LOOP_CONST, ELSEIF2_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(US, 0, 0, 96, 64, 0, LOOP_CONST, ELSEIF2_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(US, 0, 0, 64, 64, 0, LOOP_CONST, ELSEIF2_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(US, 64, 64, 64, 96, 0, LOOP_CONST, FALSE_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(US, 64, 64, 64, 32, 0, LOOP_CONST, FALSE_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(US, 64, 64, 0, 0, 0, LOOP_CONST, FALSE_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(US, 64, 64, 64, 64, 0, LOOP_CONST, FALSE_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UI, 64, 96, 64, 96, 0, LOOP_CONST, IF_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UI, 64, 96, 96, 64, 0, LOOP_CONST, IF_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UI, 64, 96, 0, 0, 0, LOOP_CONST, IF_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UI, 64, 96, 64, 64, 0, LOOP_CONST, IF_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UI, 64, 32, 96, 64, 0, LOOP_CONST, ELSEIF1_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UI, 64, 32, 64, 96, 0, LOOP_CONST, ELSEIF1_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UI, 64, 32, 0, 0, 0, LOOP_CONST, ELSEIF1_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UI, 64, 32, 64, 64, 0, LOOP_CONST, ELSEIF1_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UI, 0, 0, 0, 0, 0, LOOP_CONST, ELSEIF2_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UI, 0, 0, 64, 96, 0, LOOP_CONST, ELSEIF2_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UI, 0, 0, 96, 64, 0, LOOP_CONST, ELSEIF2_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UI, 0, 0, 64, 64, 0, LOOP_CONST, ELSEIF2_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UI, 64, 64, 64, 96, 0, LOOP_CONST, FALSE_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UI, 64, 64, 64, 32, 0, LOOP_CONST, FALSE_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UI, 64, 64, 0, 0, 0, LOOP_CONST, FALSE_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UI, 64, 64, 64, 64, 0, LOOP_CONST, FALSE_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UL, 64, 96, 64, 96, 0, LOOP_CONST, IF_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UL, 64, 96, 96, 64, 0, LOOP_CONST, IF_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UL, 64, 96, 0, 0, 0, LOOP_CONST, IF_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UL, 64, 96, 64, 64, 0, LOOP_CONST, IF_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UL, 64, 32, 96, 64, 0, LOOP_CONST, ELSEIF1_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UL, 64, 32, 64, 96, 0, LOOP_CONST, ELSEIF1_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UL, 64, 32, 0, 0, 0, LOOP_CONST, ELSEIF1_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UL, 64, 32, 64, 64, 0, LOOP_CONST, ELSEIF1_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UL, 0, 0, 0, 0, 0, LOOP_CONST, ELSEIF2_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UL, 0, 0, 64, 96, 0, LOOP_CONST, ELSEIF2_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UL, 0, 0, 96, 64, 0, LOOP_CONST, ELSEIF2_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UL, 0, 0, 64, 64, 0, LOOP_CONST, ELSEIF2_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UL, 64, 64, 64, 96, 0, LOOP_CONST, FALSE_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UL, 64, 64, 64, 32, 0, LOOP_CONST, FALSE_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UL, 64, 64, 0, 0, 0, LOOP_CONST, FALSE_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UL, 64, 64, 64, 64, 0, LOOP_CONST, FALSE_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(ULL, 64, 96, 64, 96, 0, LOOP_CONST, IF_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(ULL, 64, 96, 96, 64, 0, LOOP_CONST, IF_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(ULL, 64, 96, 0, 0, 0, LOOP_CONST, IF_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(ULL, 64, 96, 64, 64, 0, LOOP_CONST, IF_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(ULL, 64, 32, 96, 64, 0, LOOP_CONST, ELSEIF1_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(ULL, 64, 32, 64, 96, 0, LOOP_CONST, ELSEIF1_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(ULL, 64, 32, 0, 0, 0, LOOP_CONST, ELSEIF1_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(ULL, 64, 32, 64, 64, 0, LOOP_CONST, ELSEIF1_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(ULL, 0, 0, 0, 0, 0, LOOP_CONST, ELSEIF2_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(ULL, 0, 0, 64, 96, 0, LOOP_CONST, ELSEIF2_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(ULL, 0, 0, 96, 64, 0, LOOP_CONST, ELSEIF2_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(ULL, 0, 0, 64, 64, 0, LOOP_CONST, ELSEIF2_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(ULL, 64, 64, 64, 96, 0, LOOP_CONST, FALSE_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(ULL, 64, 64, 64, 32, 0, LOOP_CONST, FALSE_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(ULL, 64, 64, 0, 0, 0, LOOP_CONST, FALSE_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(ULL, 64, 64, 64, 64, 0, LOOP_CONST, FALSE_RES ,FALSE_RES);

  CALL_TEST_LOOP_NO_ESCAPE(float, 64, 96, 64, 96, 0, -1, IF_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(float, 64, 96, 96, 64, 0, -1, IF_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(float, 64, 96, 0, 0, 0, -1, IF_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(float, 64, 96, 64, 64, 0, -1, IF_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(float, 64, 32, 96, 64, 0, -1, ELSEIF1_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(float, 64, 32, 64, 96, 0, -1, ELSEIF1_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(float, 64, 32, 0, 0, 0, -1, ELSEIF1_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(float, 64, 32, 64, 64, 0, -1, ELSEIF1_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(float, 0, 0, 0, 0, 0, -1, ELSEIF2_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(float, 0, 0, 64, 96, 0, -1, ELSEIF2_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(float, 0, 0, 96, 64, 0, -1, ELSEIF2_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(float, 0, 0, 64, 64, 0, -1, ELSEIF2_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(float, 64, 64, 64, 96, 0, -1, FALSE_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(float, 64, 64, 64, 32, 0, -1, FALSE_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(float, 64, 64, 0, 0, 0, -1, FALSE_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(float, 64, 64, 64, 64, 0, -1, FALSE_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(double, 64, 96, 64, 96, 0, -1, IF_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(double, 64, 96, 96, 64, 0, -1, IF_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(double, 64, 96, 0, 0, 0, -1, IF_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(double, 64, 96, 64, 64, 0, -1, IF_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(double, 64, 32, 96, 64, 0, -1, ELSEIF1_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(double, 64, 32, 64, 96, 0, -1, ELSEIF1_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(double, 64, 32, 0, 0, 0, -1, ELSEIF1_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(double, 64, 32, 64, 64, 0, -1, ELSEIF1_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(double, 0, 0, 0, 0, 0, -1, ELSEIF2_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(double, 0, 0, 64, 96, 0, -1, ELSEIF2_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(double, 0, 0, 96, 64, 0, -1, ELSEIF2_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(double, 0, 0, 64, 64, 0, -1, ELSEIF2_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(double, 64, 64, 64, 96, 0, -1, FALSE_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(double, 64, 64, 64, 32, 0, -1, FALSE_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(double, 64, 64, 0, 0, 0, -1, FALSE_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(double, 64, 64, 64, 64, 0, -1, FALSE_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LD, 64, 96, 64, 96, 0, -1, IF_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LD, 64, 96, 96, 64, 0, -1, IF_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LD, 64, 96, 0, 0, 0, -1, IF_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LD, 64, 96, 64, 64, 0, -1, IF_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LD, 64, 32, 96, 64, 0, -1, ELSEIF1_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LD, 64, 32, 64, 96, 0, -1, ELSEIF1_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LD, 64, 32, 0, 0, 0, -1, ELSEIF1_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LD, 64, 32, 64, 64, 0, -1, ELSEIF1_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LD, 0, 0, 0, 0, 0, -1, ELSEIF2_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LD, 0, 0, 64, 96, 0, -1, ELSEIF2_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LD, 0, 0, 96, 64, 0, -1, ELSEIF2_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LD, 0, 0, 64, 64, 0, -1, ELSEIF2_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LD, 64, 64, 64, 96, 0, -1, FALSE_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LD, 64, 64, 64, 32, 0, -1, FALSE_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LD, 64, 64, 0, 0, 0, -1, FALSE_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LD, 64, 64, 64, 64, 0, -1, FALSE_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(char, 64, 96, 64, 96, 0, -1, IF_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(char, 64, 96, 96, 64, 0, -1, IF_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(char, 64, 96, 0, 0, 0, -1, IF_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(char, 64, 96, 64, 64, 0, -1, IF_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(char, 64, 32, 96, 64, 0, -1, ELSEIF1_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(char, 64, 32, 64, 96, 0, -1, ELSEIF1_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(char, 64, 32, 0, 0, 0, -1, ELSEIF1_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(char, 64, 32, 64, 64, 0, -1, ELSEIF1_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(char, 0, 0, 0, 0, 0, -1, ELSEIF2_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(char, 0, 0, 64, 96, 0, -1, ELSEIF2_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(char, 0, 0, 96, 64, 0, -1, ELSEIF2_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(char, 0, 0, 64, 64, 0, -1, ELSEIF2_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(char, 64, 64, 64, 96, 0, -1, FALSE_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(char, 64, 64, 64, 32, 0, -1, FALSE_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(char, 64, 64, 0, 0, 0, -1, FALSE_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(char, 64, 64, 64, 64, 0, -1, FALSE_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(SI, 64, 96, 64, 96, 0, -1, IF_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(SI, 64, 96, 96, 64, 0, -1, IF_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(SI, 64, 96, 0, 0, 0, -1, IF_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(SI, 64, 96, 64, 64, 0, -1, IF_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(SI, 64, 32, 96, 64, 0, -1, ELSEIF1_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(SI, 64, 32, 64, 96, 0, -1, ELSEIF1_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(SI, 64, 32, 0, 0, 0, -1, ELSEIF1_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(SI, 64, 32, 64, 64, 0, -1, ELSEIF1_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(SI, 0, 0, 0, 0, 0, -1, ELSEIF2_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(SI, 0, 0, 64, 96, 0, -1, ELSEIF2_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(SI, 0, 0, 96, 64, 0, -1, ELSEIF2_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(SI, 0, 0, 64, 64, 0, -1, ELSEIF2_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(SI, 64, 64, 64, 96, 0, -1, FALSE_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(SI, 64, 64, 64, 32, 0, -1, FALSE_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(SI, 64, 64, 0, 0, 0, -1, FALSE_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(SI, 64, 64, 64, 64, 0, -1, FALSE_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(int, 64, 96, 64, 96, 0, -1, IF_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(int, 64, 96, 96, 64, 0, -1, IF_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(int, 64, 96, 0, 0, 0, -1, IF_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(int, 64, 96, 64, 64, 0, -1, IF_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(int, 64, 32, 96, 64, 0, -1, ELSEIF1_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(int, 64, 32, 64, 96, 0, -1, ELSEIF1_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(int, 64, 32, 0, 0, 0, -1, ELSEIF1_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(int, 64, 32, 64, 64, 0, -1, ELSEIF1_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(int, 0, 0, 0, 0, 0, -1, ELSEIF2_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(int, 0, 0, 64, 96, 0, -1, ELSEIF2_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(int, 0, 0, 96, 64, 0, -1, ELSEIF2_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(int, 0, 0, 64, 64, 0, -1, ELSEIF2_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(int, 64, 64, 64, 96, 0, -1, FALSE_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(int, 64, 64, 64, 32, 0, -1, FALSE_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(int, 64, 64, 0, 0, 0, -1, FALSE_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(int, 64, 64, 64, 64, 0, -1, FALSE_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LI, 64, 96, 64, 96, 0, -1, IF_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LI, 64, 96, 96, 64, 0, -1, IF_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LI, 64, 96, 0, 0, 0, -1, IF_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LI, 64, 96, 64, 64, 0, -1, IF_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LI, 64, 32, 96, 64, 0, -1, ELSEIF1_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LI, 64, 32, 64, 96, 0, -1, ELSEIF1_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LI, 64, 32, 0, 0, 0, -1, ELSEIF1_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LI, 64, 32, 64, 64, 0, -1, ELSEIF1_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LI, 0, 0, 0, 0, 0, -1, ELSEIF2_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LI, 0, 0, 64, 96, 0, -1, ELSEIF2_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LI, 0, 0, 96, 64, 0, -1, ELSEIF2_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LI, 0, 0, 64, 64, 0, -1, ELSEIF2_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LI, 64, 64, 64, 96, 0, -1, FALSE_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LI, 64, 64, 64, 32, 0, -1, FALSE_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LI, 64, 64, 0, 0, 0, -1, FALSE_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LI, 64, 64, 64, 64, 0, -1, FALSE_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LLI, 64, 96, 64, 96, 0, -1, IF_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LLI, 64, 96, 96, 64, 0, -1, IF_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LLI, 64, 96, 0, 0, 0, -1, IF_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LLI, 64, 96, 64, 64, 0, -1, IF_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LLI, 64, 32, 96, 64, 0, -1, ELSEIF1_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LLI, 64, 32, 64, 96, 0, -1, ELSEIF1_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LLI, 64, 32, 0, 0, 0, -1, ELSEIF1_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LLI, 64, 32, 64, 64, 0, -1, ELSEIF1_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LLI, 0, 0, 0, 0, 0, -1, ELSEIF2_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LLI, 0, 0, 64, 96, 0, -1, ELSEIF2_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LLI, 0, 0, 96, 64, 0, -1, ELSEIF2_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LLI, 0, 0, 64, 64, 0, -1, ELSEIF2_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LLI, 64, 64, 64, 96, 0, -1, FALSE_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LLI, 64, 64, 64, 32, 0, -1, FALSE_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LLI, 64, 64, 0, 0, 0, -1, FALSE_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LLI, 64, 64, 64, 64, 0, -1, FALSE_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UC, 64, 96, 64, 96, 0, -1, IF_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UC, 64, 96, 96, 64, 0, -1, IF_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UC, 64, 96, 0, 0, 0, -1, IF_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UC, 64, 96, 64, 64, 0, -1, IF_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UC, 64, 32, 96, 64, 0, -1, ELSEIF1_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UC, 64, 32, 64, 96, 0, -1, ELSEIF1_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UC, 64, 32, 0, 0, 0, -1, ELSEIF1_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UC, 64, 32, 64, 64, 0, -1, ELSEIF1_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UC, 0, 0, 0, 0, 0, -1, ELSEIF2_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UC, 0, 0, 64, 96, 0, -1, ELSEIF2_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UC, 0, 0, 96, 64, 0, -1, ELSEIF2_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UC, 0, 0, 64, 64, 0, -1, ELSEIF2_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UC, 64, 64, 64, 96, 0, -1, FALSE_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UC, 64, 64, 64, 32, 0, -1, FALSE_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UC, 64, 64, 0, 0, 0, -1, FALSE_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UC, 64, 64, 64, 64, 0, -1, FALSE_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(US, 64, 96, 64, 96, 0, -1, IF_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(US, 64, 96, 96, 64, 0, -1, IF_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(US, 64, 96, 0, 0, 0, -1, IF_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(US, 64, 96, 64, 64, 0, -1, IF_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(US, 64, 32, 96, 64, 0, -1, ELSEIF1_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(US, 64, 32, 64, 96, 0, -1, ELSEIF1_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(US, 64, 32, 0, 0, 0, -1, ELSEIF1_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(US, 64, 32, 64, 64, 0, -1, ELSEIF1_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(US, 0, 0, 0, 0, 0, -1, ELSEIF2_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(US, 0, 0, 64, 96, 0, -1, ELSEIF2_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(US, 0, 0, 96, 64, 0, -1, ELSEIF2_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(US, 0, 0, 64, 64, 0, -1, ELSEIF2_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(US, 64, 64, 64, 96, 0, -1, FALSE_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(US, 64, 64, 64, 32, 0, -1, FALSE_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(US, 64, 64, 0, 0, 0, -1, FALSE_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(US, 64, 64, 64, 64, 0, -1, FALSE_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UI, 64, 96, 64, 96, 0, -1, IF_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UI, 64, 96, 96, 64, 0, -1, IF_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UI, 64, 96, 0, 0, 0, -1, IF_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UI, 64, 96, 64, 64, 0, -1, IF_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UI, 64, 32, 96, 64, 0, -1, ELSEIF1_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UI, 64, 32, 64, 96, 0, -1, ELSEIF1_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UI, 64, 32, 0, 0, 0, -1, ELSEIF1_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UI, 64, 32, 64, 64, 0, -1, ELSEIF1_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UI, 0, 0, 0, 0, 0, -1, ELSEIF2_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UI, 0, 0, 64, 96, 0, -1, ELSEIF2_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UI, 0, 0, 96, 64, 0, -1, ELSEIF2_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UI, 0, 0, 64, 64, 0, -1, ELSEIF2_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UI, 64, 64, 64, 96, 0, -1, FALSE_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UI, 64, 64, 64, 32, 0, -1, FALSE_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UI, 64, 64, 0, 0, 0, -1, FALSE_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UI, 64, 64, 64, 64, 0, -1, FALSE_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UL, 64, 96, 64, 96, 0, -1, IF_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UL, 64, 96, 96, 64, 0, -1, IF_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UL, 64, 96, 0, 0, 0, -1, IF_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UL, 64, 96, 64, 64, 0, -1, IF_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UL, 64, 32, 96, 64, 0, -1, ELSEIF1_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UL, 64, 32, 64, 96, 0, -1, ELSEIF1_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UL, 64, 32, 0, 0, 0, -1, ELSEIF1_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UL, 64, 32, 64, 64, 0, -1, ELSEIF1_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UL, 0, 0, 0, 0, 0, -1, ELSEIF2_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UL, 0, 0, 64, 96, 0, -1, ELSEIF2_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UL, 0, 0, 96, 64, 0, -1, ELSEIF2_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UL, 0, 0, 64, 64, 0, -1, ELSEIF2_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UL, 64, 64, 64, 96, 0, -1, FALSE_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UL, 64, 64, 64, 32, 0, -1, FALSE_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UL, 64, 64, 0, 0, 0, -1, FALSE_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UL, 64, 64, 64, 64, 0, -1, FALSE_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(ULL, 64, 96, 64, 96, 0, -1, IF_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(ULL, 64, 96, 96, 64, 0, -1, IF_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(ULL, 64, 96, 0, 0, 0, -1, IF_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(ULL, 64, 96, 64, 64, 0, -1, IF_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(ULL, 64, 32, 96, 64, 0, -1, ELSEIF1_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(ULL, 64, 32, 64, 96, 0, -1, ELSEIF1_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(ULL, 64, 32, 0, 0, 0, -1, ELSEIF1_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(ULL, 64, 32, 64, 64, 0, -1, ELSEIF1_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(ULL, 0, 0, 0, 0, 0, -1, ELSEIF2_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(ULL, 0, 0, 64, 96, 0, -1, ELSEIF2_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(ULL, 0, 0, 96, 64, 0, -1, ELSEIF2_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(ULL, 0, 0, 64, 64, 0, -1, ELSEIF2_RES ,FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(ULL, 64, 64, 64, 96, 0, -1, FALSE_RES ,IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(ULL, 64, 64, 64, 32, 0, -1, FALSE_RES ,ELSEIF1_RES);
  CALL_TEST_LOOP_NO_ESCAPE(ULL, 64, 64, 0, 0, 0, -1, FALSE_RES ,ELSEIF2_RES);
  CALL_TEST_LOOP_NO_ESCAPE(ULL, 64, 64, 64, 64, 0, -1, FALSE_RES ,FALSE_RES);

  puts("test finish");
  return 0;
}
