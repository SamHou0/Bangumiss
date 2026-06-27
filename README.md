# Bangumiss
A bridge to make Bangumi -> Misskey (and their forks) possible.

Built for those who still like Bangumi after creating fediverse accounts. NeoDB is great, but bgmers are interesting too, right?

<img width="1601" alt="Bangumiss Preview" src="https://github.com/user-attachments/assets/f47d240f-f34e-4ee5-aa68-51d61b6f61e7" />


## Feature

- When changing watching/playing status, automatically post on misskey
- Save entry to db to avoid spamming on timelines
- Post Visibility adjustable!
- Hide spoilers with blur

## Preview
You can see an example bot account at [@bangumiss@clanna.dev](https://clanna.dev/@bangumiss)

## Quick Start

First pull this repo:

```bash
git pull https://github.com/SamHou0/Bangumiss.git
cd Bangumiss
```

Then let's make a data dir and get right permission:

```bash
mkdir data
cd data
touch bangumiss.db
chmod 660 bangumiss.db
chmod 770 .
cd ..
```

Then edit `.env`

Note: `.env` is in an subfolder. So cd Bangumiss again is necessary.

```bash
cd Bangumiss
cp .env.example .env
nano .env
```

Edit .env, according to your need. You may generate a key on your misskey instance, and a bangumi key [here](https://next.bgm.tv/demo/access-token).

Then run compose and enjoy!

```bash
docker compose up -d --build && docker compose logs -f
```
