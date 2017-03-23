CREATE ROLE politicalpurse LOGIN PASSWORD 'Password1';

CREATE DATABASE politicalpurse
  WITH ENCODING='UTF8'
       OWNER=politicalpurse
       CONNECTION LIMIT=-1;

GRANT ALL ON DATABASE politicalpurse TO politicalpurse;