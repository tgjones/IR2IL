
#include <stdio.h>

#define LOOP_CONST 64
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
}flag;

#define TEST_LOOP_NO_ESCAPE_PT1(test_type) \
int test_loop_no_escape_pt1_##test_type(struct fstruct flag, int lmax, test_type d_val) {\
  int i;\
  test_type d_data1[LOOP_CONST], d_data2[LOOP_CONST];\
  initialize_##test_type(d_data1, LOOP_CONST);\
  initialize_##test_type(d_data2, LOOP_CONST);\
  for(i=0; i<LOOP_CONST; i++) {\
    if ((test_type)flag.flag1 > (test_type)flag.flag2) {\
      d_data1[i] = (test_type)IF_RES;\
    } else if (i < (test_type)flag.flag3) {\
      d_data1[i] = (test_type)ELSEIF1_RES;\
    } else if ((test_type)flag.flag1 == (test_type)flag.flag2) {\
      d_data1[i] = (test_type)ELSEIF2_RES;\
    }\
    d_data2[i] = (test_type)IF_RES;\
  }\
  for(i=0;i<LOOP_CONST;i++) {\
    if (i < (test_type)flag.flag3) {\
      if (d_data1[i] != (test_type)ELSEIF1_RES) {\
        return FALSE_RES;\
      }\
    } else {\
      if (d_data1[i] != d_val) {\
        return FALSE_RES;\
      }\
    }\
    if (d_data2[i] != (test_type)IF_RES) {\
      return FALSE_RES;\
    }\
  }\
  return TRUE_RES;\
}

#define TEST_LOOP_NO_ESCAPE_PT2(test_type) \
int test_loop_no_escape_pt2_##test_type(struct fstruct flag, int lmax, test_type d_val) {\
  int i;\
  test_type d_data1[LOOP_CONST], d_data2[LOOP_CONST];\
  initialize_##test_type(d_data1, LOOP_CONST);\
  initialize_##test_type(d_data2, LOOP_CONST);\
  for(i=0; i<lmax; i++) {\
    if ((test_type)flag.flag1 > (test_type)flag.flag2) {\
      d_data1[i] = (test_type)IF_RES;\
    } else if (i < (test_type)flag.flag3) {\
      d_data1[i] = (test_type)ELSEIF1_RES;\
    } else if ((test_type)flag.flag1 == (test_type)flag.flag2) {\
      d_data1[i] = (test_type)ELSEIF2_RES;\
    }\
    d_data2[i] = (test_type)IF_RES;\
  }\
  for(i=0;i<LOOP_CONST;i++) {\
    if (lmax > 0) {\
      if (i < (test_type)flag.flag3) {\
        if (d_data1[i] != (test_type)ELSEIF1_RES) {\
          return FALSE_RES;\
        }\
      } else {\
        if (d_data1[i] != d_val) {\
          return FALSE_RES;\
        }\
      }\
      if (d_data2[i] != (test_type)IF_RES) {\
        return FALSE_RES;\
      }\
    } else {\
      if (d_data1[i] != (test_type)FALSE_RES || d_data2[i] != (test_type)FALSE_RES) {\
        return FALSE_RES;\
      }\
    }\
  }\
  return TRUE_RES;\
}

#define TEST_LOOP_NO_ESCAPE_PT3(test_type) \
int test_loop_no_escape_pt3_##test_type(struct fstruct flag, int lmax, test_type d_val) {\
  int i;\
  test_type d_data1[LOOP_CONST], d_data2[LOOP_CONST];\
  initialize_##test_type(d_data1, LOOP_CONST);\
  initialize_##test_type(d_data2, LOOP_CONST);\
  for(i=0; i<LOOP_CONST; i++) {\
    if (i < (test_type)flag.flag3) {\
      d_data1[i] = (test_type)ELSEIF1_RES;\
    } else if ((test_type)flag.flag1 > (test_type)flag.flag2) {\
      d_data1[i] = (test_type)IF_RES;\
    } else if ((test_type)flag.flag1 == (test_type)flag.flag2) {\
      d_data1[i] = (test_type)ELSEIF2_RES;\
    }\
    d_data2[i] = (test_type)IF_RES;\
  }\
  for(i=0;i<LOOP_CONST;i++) {\
    if (i < (test_type)flag.flag3) {\
      if (d_data1[i] != (test_type)ELSEIF1_RES) {\
        return FALSE_RES;\
      }\
    } else {\
      if (d_data1[i] != d_val) {\
        return FALSE_RES;\
      }\
    }\
    if (d_data2[i] != (test_type)IF_RES) {\
      return FALSE_RES;\
    }\
  }\
  return TRUE_RES;\
}

#define TEST_LOOP_NO_ESCAPE_PT4(test_type) \
int test_loop_no_escape_pt4_##test_type(struct fstruct flag, int lmax, test_type d_val) {\
  int i;\
  test_type d_data1[LOOP_CONST], d_data2[LOOP_CONST];\
  initialize_##test_type(d_data1, LOOP_CONST);\
  initialize_##test_type(d_data2, LOOP_CONST);\
  for(i=0; i<lmax; i++) {\
    if (i < (test_type)flag.flag3) {\
      d_data1[i] = (test_type)ELSEIF1_RES;\
    } else if ((test_type)flag.flag1 > (test_type)flag.flag2) {\
      d_data1[i] = (test_type)IF_RES;\
    } else if ((test_type)flag.flag1 == (test_type)flag.flag2) {\
      d_data1[i] = (test_type)ELSEIF2_RES;\
    }\
    d_data2[i] = (test_type)IF_RES;\
  }\
  for(i=0;i<LOOP_CONST;i++) {\
    if (lmax > 0) {\
      if (i < (test_type)flag.flag3) {\
        if (d_data1[i] != (test_type)ELSEIF1_RES) {\
          return FALSE_RES;\
        }\
      } else {\
        if (d_data1[i] != d_val) {\
          return FALSE_RES;\
        }\
      }\
      if (d_data2[i] != (test_type)IF_RES) {\
        return FALSE_RES;\
      }\
    } else {\
      if (d_data1[i] != (test_type)FALSE_RES || d_data2[i] != (test_type)FALSE_RES) {\
        return FALSE_RES;\
      }\
    }\
  }\
  return TRUE_RES;\
}

#define CALL_TEST_LOOP_NO_ESCAPE(test_type,flag_value1, flag_value2, flag_value3, loop_max, comp_value) \
flag.flag1 = flag_value1; flag.flag2 = flag_value2; flag.flag3 = flag_value3; \
if (!test_loop_no_escape_pt1_##test_type(flag,loop_max,(test_type)comp_value) ||\
    !test_loop_no_escape_pt2_##test_type(flag,loop_max,(test_type)comp_value) ||\
    !test_loop_no_escape_pt3_##test_type(flag,loop_max,(test_type)comp_value) ||\
    !test_loop_no_escape_pt4_##test_type(flag,loop_max,(test_type)comp_value)) puts("NG : "#test_type);

INITIALIZE_METHOD(float);
TEST_LOOP_NO_ESCAPE_PT1(float);
TEST_LOOP_NO_ESCAPE_PT2(float);
TEST_LOOP_NO_ESCAPE_PT3(float);
TEST_LOOP_NO_ESCAPE_PT4(float);

INITIALIZE_METHOD(double);
TEST_LOOP_NO_ESCAPE_PT1(double);
TEST_LOOP_NO_ESCAPE_PT2(double);
TEST_LOOP_NO_ESCAPE_PT3(double);
TEST_LOOP_NO_ESCAPE_PT4(double);

typedef long double LD;
INITIALIZE_METHOD(LD);
TEST_LOOP_NO_ESCAPE_PT1(LD);
TEST_LOOP_NO_ESCAPE_PT2(LD);
TEST_LOOP_NO_ESCAPE_PT3(LD);
TEST_LOOP_NO_ESCAPE_PT4(LD);

INITIALIZE_METHOD(char);
TEST_LOOP_NO_ESCAPE_PT1(char);
TEST_LOOP_NO_ESCAPE_PT2(char);
TEST_LOOP_NO_ESCAPE_PT3(char);
TEST_LOOP_NO_ESCAPE_PT4(char);

typedef short int SI;
INITIALIZE_METHOD(SI);
TEST_LOOP_NO_ESCAPE_PT1(SI);
TEST_LOOP_NO_ESCAPE_PT2(SI);
TEST_LOOP_NO_ESCAPE_PT3(SI);
TEST_LOOP_NO_ESCAPE_PT4(SI);

INITIALIZE_METHOD(int);
TEST_LOOP_NO_ESCAPE_PT1(int);
TEST_LOOP_NO_ESCAPE_PT2(int);
TEST_LOOP_NO_ESCAPE_PT3(int);
TEST_LOOP_NO_ESCAPE_PT4(int);

typedef long int LI;
INITIALIZE_METHOD(LI);
TEST_LOOP_NO_ESCAPE_PT1(LI);
TEST_LOOP_NO_ESCAPE_PT2(LI);
TEST_LOOP_NO_ESCAPE_PT3(LI);
TEST_LOOP_NO_ESCAPE_PT4(LI);

typedef long long int LLI;
INITIALIZE_METHOD(LLI);
TEST_LOOP_NO_ESCAPE_PT1(LLI);
TEST_LOOP_NO_ESCAPE_PT2(LLI);
TEST_LOOP_NO_ESCAPE_PT3(LLI);
TEST_LOOP_NO_ESCAPE_PT4(LLI);

typedef unsigned char UC;
INITIALIZE_METHOD(UC);
TEST_LOOP_NO_ESCAPE_PT1(UC);
TEST_LOOP_NO_ESCAPE_PT2(UC);
TEST_LOOP_NO_ESCAPE_PT3(UC);
TEST_LOOP_NO_ESCAPE_PT4(UC);

typedef unsigned char US;
INITIALIZE_METHOD(US);
TEST_LOOP_NO_ESCAPE_PT1(US);
TEST_LOOP_NO_ESCAPE_PT2(US);
TEST_LOOP_NO_ESCAPE_PT3(US);
TEST_LOOP_NO_ESCAPE_PT4(US);

typedef unsigned int UI;
INITIALIZE_METHOD(UI);
TEST_LOOP_NO_ESCAPE_PT1(UI);
TEST_LOOP_NO_ESCAPE_PT2(UI);
TEST_LOOP_NO_ESCAPE_PT3(UI);
TEST_LOOP_NO_ESCAPE_PT4(UI);

typedef unsigned long int UL;
INITIALIZE_METHOD(UL);
TEST_LOOP_NO_ESCAPE_PT1(UL);
TEST_LOOP_NO_ESCAPE_PT2(UL);
TEST_LOOP_NO_ESCAPE_PT3(UL);
TEST_LOOP_NO_ESCAPE_PT4(UL);

typedef unsigned long long int ULL;
INITIALIZE_METHOD(ULL);
TEST_LOOP_NO_ESCAPE_PT1(ULL);
TEST_LOOP_NO_ESCAPE_PT2(ULL);
TEST_LOOP_NO_ESCAPE_PT3(ULL);
TEST_LOOP_NO_ESCAPE_PT4(ULL);

int main() {
  CALL_TEST_LOOP_NO_ESCAPE(float, 2, 1, 0, LOOP_CONST, IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(float, 1, 2, 32, LOOP_CONST, FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(float, 1, 2, 0, LOOP_CONST, FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(double, 2, 1, 0, LOOP_CONST, IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(double, 1, 2, 32, LOOP_CONST, FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(double, 1, 2, 0, LOOP_CONST, FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LD, 2, 1, 0, LOOP_CONST, IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LD, 1, 2, 32, LOOP_CONST, FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LD, 1, 2, 0, LOOP_CONST, FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(char, 2, 1, 0, LOOP_CONST, IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(char, 1, 2, 32, LOOP_CONST, FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(char, 1, 2, 0, LOOP_CONST, FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(SI, 2, 1, 0, LOOP_CONST, IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(SI, 1, 2, 32, LOOP_CONST, FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(SI, 1, 2, 0, LOOP_CONST, FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(int, 2, 1, 0, LOOP_CONST, IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(int, 1, 2, 32, LOOP_CONST, FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(int, 1, 2, 0, LOOP_CONST, FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LI, 2, 1, 0, LOOP_CONST, IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LI, 1, 2, 32, LOOP_CONST, FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LI, 1, 2, 0, LOOP_CONST, FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LLI, 2, 1, 0, LOOP_CONST, IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LLI, 1, 2, 32, LOOP_CONST, FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LLI, 1, 2, 0, LOOP_CONST, FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UC, 2, 1, 0, LOOP_CONST, IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UC, 1, 2, 32, LOOP_CONST, FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UC, 1, 2, 0, LOOP_CONST, FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(US, 2, 1, 0, LOOP_CONST, IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(US, 1, 2, 32, LOOP_CONST, FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(US, 1, 2, 0, LOOP_CONST, FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UI, 2, 1, 0, LOOP_CONST, IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UI, 1, 2, 32, LOOP_CONST, FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UI, 1, 2, 0, LOOP_CONST, FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UL, 2, 1, 0, LOOP_CONST, IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UL, 1, 2, 32, LOOP_CONST, FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UL, 1, 2, 0, LOOP_CONST, FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(ULL, 2, 1, 0, LOOP_CONST, IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(ULL, 1, 2, 32, LOOP_CONST, FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(ULL, 1, 2, 0, LOOP_CONST, FALSE_RES);

  CALL_TEST_LOOP_NO_ESCAPE(float, 2, 1, 0, -1, IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(float, 1, 2, 32, -1, FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(float, 1, 2, 0, -1, FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(double, 2, 1, 0, -1, IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(double, 1, 2, 32, -1, FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(double, 1, 2, 0, -1, FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LD, 2, 1, 0, -1, IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LD, 1, 2, 32, -1, FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LD, 1, 2, 0, -1, FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(char, 2, 1, 0, -1, IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(char, 1, 2, 32, -1, FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(char, 1, 2, 0, -1, FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(SI, 2, 1, 0, -1, IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(SI, 1, 2, 32, -1, FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(SI, 1, 2, 0, -1, FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(int, 2, 1, 0, -1, IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(int, 1, 2, 32, -1, FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(int, 1, 2, 0, -1, FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LI, 2, 1, 0, -1, IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LI, 1, 2, 32, -1, FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LI, 1, 2, 0, -1, FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LLI, 2, 1, 0, -1, IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LLI, 1, 2, 32, -1, FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(LLI, 1, 2, 0, -1, FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UC, 2, 1, 0, -1, IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UC, 1, 2, 32, -1, FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UC, 1, 2, 0, -1, FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(US, 2, 1, 0, -1, IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(US, 1, 2, 32, -1, FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(US, 1, 2, 0, -1, FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UI, 2, 1, 0, -1, IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UI, 1, 2, 32, -1, FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UI, 1, 2, 0, -1, FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UL, 2, 1, 0, -1, IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UL, 1, 2, 32, -1, FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(UL, 1, 2, 0, -1, FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(ULL, 2, 1, 0, -1, IF_RES);
  CALL_TEST_LOOP_NO_ESCAPE(ULL, 1, 2, 32, -1, FALSE_RES);
  CALL_TEST_LOOP_NO_ESCAPE(ULL, 1, 2, 0, -1, FALSE_RES);

  puts("test finish");
  return 0;
}
