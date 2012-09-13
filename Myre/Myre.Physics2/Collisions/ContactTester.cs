//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace Myre.Physics.Collisions
//{
//    public class ContactTester
//    {
//        private SatTester sat;
//        private int maxContacts;

//        public ContactTester(int maxContacts = 8)
//        {
//            this.sat = new SatTester();
//            this.maxContacts = maxContacts;
//        }

//        public void FindContacts(Geometry a, Geometry b, List<Contact> contacts)
//        {
//            SatResult? r = sat.FindIntersection(a, b);

//            if (r == null)
//                return;

//            SatResult result = r.Value;

//            var aVertices = a.GetVertices(result.NormalAxis);
//            for (int i = 0; i < aVertices.Length; i++)
//            {
//                if (contacts.Count == maxContacts)
//                    break;

//                if (b.Contains(aVertices[i]))
//                {
//                    contacts.Add(new Contact()
//                    {
//                        A = a,
//                        B = b,
//                        Normal = result.NormalAxis,
//                        Position = aVertices[i],
//                        PenetrationDepth = result.Penetration
//                    });
//                }
//            }

//            var bVertices = b.GetVertices(-result.NormalAxis);
//            for (int i = 0; i < bVertices.Length; i++)
//            {
//                if (contacts.Count == maxContacts)
//                    break;

//                if (a.Contains(bVertices[i]))
//                {
//                    contacts.Add(new Contact() {
//                        A = a,
//                        B = b,
//                        Normal = result.NormalAxis,
//                        Position = bVertices[i],
//                        PenetrationDepth = result.Penetration
//                    });
//                }
//            }

//            if (contacts.Count == 0)
//            {
//                contacts.Add(new Contact() {
//                    A = a,
//                    B = b,
//                    Normal = result.NormalAxis,
//                    Position = result.DeepestPoint,
//                    PenetrationDepth = result.Penetration
//                });
//            }
//        }
//    }
//}
