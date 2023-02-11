/*
 Navicat Premium Data Transfer

 Source Server         : postgregeneral
 Source Server Type    : PostgreSQL
 Source Server Version : 100022
 Source Host           : 46.101.146.148:5432
 Source Catalog        : digit_case
 Source Schema         : public

 Target Server Type    : PostgreSQL
 Target Server Version : 100022
 File Encoding         : 65001

 Date: 12/02/2023 02:22:15
*/


-- ----------------------------
-- Table structure for admin
-- ----------------------------
DROP TABLE IF EXISTS "public"."admin";
CREATE TABLE "public"."admin" (
  "id" int4 NOT NULL DEFAULT nextval('admin_id_seq'::regclass),
  "password" varchar(255) COLLATE "pg_catalog"."default",
  "name" varchar(255) COLLATE "pg_catalog"."default",
  "surname" varchar(255) COLLATE "pg_catalog"."default",
  "email" varchar(255) COLLATE "pg_catalog"."default",
  "create_at" timestamp(0),
  "password_hash" bytea,
  "password_salt" bytea,
  "account_confirm" bool DEFAULT false,
  "tmp_password" varchar(255) COLLATE "pg_catalog"."default",
  "confirm_code" varchar(255) COLLATE "pg_catalog"."default",
  "token" text COLLATE "pg_catalog"."default",
  "is_online" bool DEFAULT false
)
;

-- ----------------------------
-- Records of admin
-- ----------------------------
INSERT INTO "public"."admin" VALUES (22, 'qweasdzxc', 'Şevket', 'KAPLAN', 'sevketkaplan@outlook.com', '2023-02-12 01:04:55', E'\\310j3\\361gIv\\322\\263?Kcd\\210\\\\S*\\240\\257\\036\\361}\\325\\243\\310\\243\\232\\206?h\\223\\273"&M[@''\\364A\\321\\321\\031\\375\\347\\273\\312\\000\\245\\006\\212\\271\\\\\\313\\234B\\267\\002\\232a$2\\212k', E'\\325g\\256\\206h\\233 ]\\016K\\247\\205\\216Sw\\216\\207\\312=\\031\\360]\\222\\323S\\021\\203,\\264L\\270\\017\\363ut\\032\\264\\317G?\\330\\277\\235-\\001x\\273\\033\\354\\367\\024\\250\\031P\\177\\023\\235y\\006w\\005\\275f\\274\\350/\\335\\323\\246\\371A\\216\\365\\337\\347\\311C\\255\\024\\313\\350\\320\\0323\\302),\\326\\353P=c\\317\\026i)\\230)\\242\\034\\324\\2103\\342H\\344\\201\\01692Sd\\017z{~\\352\\365!W\\344\\326\\027\\202\\3008\\264\\023', 'f', NULL, '287429', 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IjAiLCJuYmYiOjE2NzYxNTMwOTQsImV4cCI6MTY3Njc1Nzg5NCwiaWF0IjoxNjc2MTUzMDk0fQ.z7Xu8S63uALvxkznxihPKnzJoK6vUDCFzL27l5ZkI9I', 'f');
INSERT INTO "public"."admin" VALUES (1, 'qweasdzxc', 'Şevket', 'KAPLAN', 'sevketkaplan.20@gmail.com', '2023-02-11 00:00:00', E'*p\\352\\352K\\205\\357\\316\\321J\\220g\\342erj\\210|\\220\\252\\256PG\\206\\323H5m\\311?D\\0345\\021*\\255Mr\\232\\250h\\336At\\201\\250v\\341^h\\212\\217\\034|\\263\\350\\272L\\250\\177\\303W\\231\\343', E'\\250\\346j0?\\3766\\346{^\\326z!\\254\\202\\211\\2631CO\\023\\002\\276\\371C\\022\\371\\257\\262\\254\\351k*\\311\\261\\210\\322\\220!\\000_\\000-\\022\\367V\\307\\230\\214\\267\\205\\321\\307/\\256\\025-\\223\\273\\357T\\317fzV\\220''@\\362e\\214\\333\\314\\210NX\\277\\036\\273u>QM\\016\\361\\364\\376<\\303Y\\356\\221\\21283\\372\\200Q\\303\\267`\\023\\354\\013)\\272\\033\\251\\250t\\267s.p\\307W\\316 ;6e4\\273I?u\\244\\025', 't', NULL, NULL, 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IjAiLCJuYmYiOjE2NDkxNDg0MzUsImV4cCI6MTY0OTc1MzIzNSwiaWF0IjoxNjQ5MTQ4NDM1fQ.wwRYd3cTEbxIglGrKsWlzzZrrZoUIw-ihPZ0zd3HTd4', 't');

-- ----------------------------
-- Primary Key structure for table admin
-- ----------------------------
ALTER TABLE "public"."admin" ADD CONSTRAINT "admin_copy1_pkey" PRIMARY KEY ("id");
