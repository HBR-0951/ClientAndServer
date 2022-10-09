import pymongo
from pymongo import MongoClient

# Connect
conn = "mongodb+srv://HBR-0951:<password>@cluster0.4n5gfld.mongodb.net/?retryWrites=true&w=majority"
client = MongoClient(conn)

db = client.get_database('student_db')
records = db.student_records

# use document

# 加一個
# new_student = {'name': "wendy",
#                 'roll_no': 132,
#                 "branch": "wsw"
#                 }
#
# records.insert_one(new_student)

# 加多個
# new_students = [
#     {'name': "wendy",
#      'roll_no': 132,
#      "branch": "wsw"
#      },
#     {
#         'name': "ken",
#         'roll_no': 32,
#         'branch': "tre"
#     }]
#
# records.insert_many(new_students)


# print data
cursor = records.find({})
data = [d for d in cursor]
print(data)

# # count document
# count = records.count_documents({})
# print(count)

# find documents
# alist = list(records.find())
#
# afind = records.find_one({'roll_no':123})
# print(afind)

# update documents
# student_updates = {'name': 'david'}
# records.update_one({'roll_no': 123}, {'$set': student_updates})

# delete documents
# records.delete_one({'roll_no': 123})
