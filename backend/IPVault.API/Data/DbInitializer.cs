using Microsoft.EntityFrameworkCore;
using IPVault.API.Models;

namespace IPVault.API.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Ensure Database is created
        await context.Database.EnsureCreatedAsync();

        // 1. Seed Users
        if (!await context.Users.AnyAsync())
        {
            var users = new List<User>
            {
                new()
                {
                    FullName = "Vikram Sharma (System Administrator)",
                    Email = "admin@ipvault.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    Role = "Admin",
                    CreatedAt = DateTime.UtcNow.AddMonths(-12)
                },
                new()
                {
                    FullName = "Ananya Iyer (Senior Patent Attorney)",
                    Email = "attorney@ipvault.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Attorney@123"),
                    Role = "Attorney",
                    CreatedAt = DateTime.UtcNow.AddMonths(-10)
                },
                new()
                {
                    FullName = "Rohan Verma (IP Paralegal Staff)",
                    Email = "staff@ipvault.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Staff@123"),
                    Role = "Staff",
                    CreatedAt = DateTime.UtcNow.AddMonths(-6)
                }
            };
            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();
        }

        // 2. Seed Patent Statuses
        if (!await context.PatentStatuses.AnyAsync())
        {
            var statuses = new List<PatentStatus>
            {
                new() { StatusName = "Draft", Description = "Initial invention disclosure and drafting stage prior to formal filing." },
                new() { StatusName = "Filed", Description = "Formally submitted to the patent office; pending initial review." },
                new() { StatusName = "Under Examination", Description = "Substantive technical examination in progress by patent examiners." },
                new() { StatusName = "Published", Description = "Published in official patent journal (usually 18 months post-filing)." },
                new() { StatusName = "Granted", Description = "Patent officially granted and enforceable legal monopoly active." },
                new() { StatusName = "Rejected", Description = "Application rejected by examiner after response or hearing." },
                new() { StatusName = "Expired", Description = "Patent term (20 years) completed or lapsed for non-payment of annuities." },
                new() { StatusName = "Abandoned", Description = "Application abandoned by applicant due to commercial decision or deadline lapse." }
            };
            await context.PatentStatuses.AddRangeAsync(statuses);
            await context.SaveChangesAsync();
        }

        // 3. Seed Clients
        if (!await context.Clients.AnyAsync())
        {
            var clients = new List<Client>
            {
                new() { ClientName = "ABC Technologies Pvt Ltd", Email = "legal@abctech.in", Phone = "+91 98230 11223", Company = "ABC Technologies Pvt Ltd", Address = "Electronic City, Phase 1, Bangalore 560100" },
                new() { ClientName = "Nirma Innovations Pvt Ltd", Email = "ip@nirmainno.com", Phone = "+91 98450 44332", Company = "Nirma Innovations Pvt Ltd", Address = "S.G. Highway, Ahmedabad 382481" },
                new() { ClientName = "TechNova Solutions Ltd", Email = "patents@technova.io", Phone = "+91 97110 55667", Company = "TechNova Solutions Ltd", Address = "Cyber City, DLF Phase 2, Gurugram 122002" },
                new() { ClientName = "Shah Legal Associates", Email = "contact@shahlegal.com", Phone = "+91 98200 99881", Company = "Shah Legal Associates", Address = "Nariman Point, Mumbai 400021" },
                new() { ClientName = "BioGenix Health Therapeutics", Email = "r_d@biogenixhealth.com", Phone = "+91 99001 77665", Company = "BioGenix Health Therapeutics", Address = "Genome Valley, Shamirpet, Hyderabad 500078" },
                new() { ClientName = "SolarCore Clean Power Corp", Email = "energy@solarcore.in", Phone = "+91 98332 11990", Company = "SolarCore Clean Power Corp", Address = "Anna Salai, Guindy, Chennai 600032" },
                new() { ClientName = "Apex Robotics & Automation", Email = "ip-team@apexrobotics.com", Phone = "+91 97654 32100", Company = "Apex Robotics & Automation", Address = "Hinjawadi IT Park, Pune 411057" },
                new() { ClientName = "QuantumChip Microelectronics", Email = "patents@quantumchip.in", Phone = "+91 91234 88776", Company = "QuantumChip Microelectronics", Address = "Whitefield Tech Corridor, Bangalore 560066" },
                new() { ClientName = "Aegis Cyber Defense Systems", Email = "security@aegisdefense.com", Phone = "+91 98112 33445", Company = "Aegis Cyber Defense Systems", Address = "Salt Lake Sector V, Kolkata 700091" },
                new() { ClientName = "Nexa EV Mobility Systems", Email = "mobility@nexaev.in", Phone = "+91 99887 76655", Company = "Nexa EV Mobility Systems", Address = "Peenya Industrial Area, Bangalore 560058" }
            };
            await context.Clients.AddRangeAsync(clients);
            await context.SaveChangesAsync();
        }

        // 4. Seed Inventors
        if (!await context.Inventors.AnyAsync())
        {
            var inventors = new List<Inventor>
            {
                new() { FirstName = "Dr. Rajesh", LastName = "Kulkarni", Email = "rajesh.k@abctech.in", Phone = "+91 98231 00001", Organization = "ABC Technologies AI Labs" },
                new() { FirstName = "Priya", LastName = "Nair", Email = "priya.n@abctech.in", Phone = "+91 98231 00002", Organization = "ABC Technologies Cloud Division" },
                new() { FirstName = "Dr. Amit", LastName = "Deshmukh", Email = "amit.d@nirmainno.com", Phone = "+91 98451 00003", Organization = "Nirma Materials Science" },
                new() { FirstName = "Sunita", LastName = "Patel", Email = "sunita.p@nirmainno.com", Phone = "+91 98451 00004", Organization = "Nirma Polymer Engineering" },
                new() { FirstName = "Arjun", LastName = "Mehta", Email = "arjun.m@technova.io", Phone = "+91 97111 00005", Organization = "TechNova Distributed Systems" },
                new() { FirstName = "Sneha", LastName = "Kapoor", Email = "sneha.k@technova.io", Phone = "+91 97111 00006", Organization = "TechNova Vision Systems" },
                new() { FirstName = "Dr. Suresh", LastName = "Menon", Email = "suresh.m@biogenixhealth.com", Phone = "+91 99002 00007", Organization = "BioGenix Immunology" },
                new() { FirstName = "Kavita", LastName = "Rao", Email = "kavita.r@biogenixhealth.com", Phone = "+91 99002 00008", Organization = "BioGenix Molecular Diagnostics" },
                new() { FirstName = "Vikrant", LastName = "Singhal", Email = "vikrant.s@solarcore.in", Phone = "+91 98333 00009", Organization = "SolarCore Photovoltaics" },
                new() { FirstName = "Divya", LastName = "Chawla", Email = "divya.c@solarcore.in", Phone = "+91 98333 00010", Organization = "SolarCore Grid Tech" },
                new() { FirstName = "Pranav", LastName = "Bhatt", Email = "pranav.b@apexrobotics.com", Phone = "+91 97655 00011", Organization = "Apex Kinematics Lab" },
                new() { FirstName = "Anjali", LastName = "Joshi", Email = "anjali.j@quantumchip.in", Phone = "+91 91235 00012", Organization = "QuantumChip Nanofabrication" },
                new() { FirstName = "Manish", LastName = "Gupta", Email = "manish.g@aegisdefense.com", Phone = "+91 98113 00013", Organization = "Aegis Cryptography Research" },
                new() { FirstName = "Harish", LastName = "Shetty", Email = "harish.s@nexaev.in", Phone = "+91 99888 00014", Organization = "Nexa Battery Dynamics" },
                new() { FirstName = "Pooja", LastName = "Hegde", Email = "pooja.h@nexaev.in", Phone = "+91 99888 00015", Organization = "Nexa Powertrain Controls" }
            };
            await context.Inventors.AddRangeAsync(inventors);
            await context.SaveChangesAsync();
        }

        // 5. Seed Patents
        if (!await context.Patents.AnyAsync())
        {
            var statusMap = await context.PatentStatuses.ToDictionaryAsync(s => s.StatusName, s => s.StatusId);
            var clients = await context.Clients.OrderBy(c => c.ClientId).ToListAsync();
            var inventors = await context.Inventors.OrderBy(i => i.InventorId).ToListAsync();

            var patentData = new List<(string num, string title, string abs, DateTime filed, DateTime? pub, DateTime? grant, DateTime? exp, string status, int clientIdx, int primaryInvIdx, int[] coInvIdxs)>
            {
                // 1
                ("IN202311001234", "AI-Driven Real-Time Urban Traffic Optimization and Emergency Vehicle Preemption",
                 "A machine-learning traffic signaling method that aggregates IoT camera feeds and coordinates signal phase timings across metropolitan intersections.",
                 new DateTime(2023, 1, 15), new DateTime(2024, 7, 15), new DateTime(2025, 2, 10), new DateTime(2043, 1, 15), "Granted", 0, 0, new[] { 1 }),

                // 2
                ("IN202311002345", "Ultra-Low Power Edge Inference Engine for Distributed IoT Sensor Nodes",
                 "A specialized quantized neural network hardware architecture minimizing memory footprint and power consumption during localized edge inference.",
                 new DateTime(2023, 3, 20), new DateTime(2024, 9, 20), null, new DateTime(2043, 3, 20), "Under Examination", 0, 1, new[] { 0 }),

                // 3
                ("IN202411003456", "High-Efficiency Perovskite-Silicon Tandem Solar Photovoltaic Cell Structure",
                 "A multi-junction photovoltaic cell incorporating passivated contact layers and novel anti-reflective texturing to achieve conversion efficiency exceeding 29%.",
                 new DateTime(2024, 2, 10), new DateTime(2025, 8, 10), null, new DateTime(2044, 2, 10), "Published", 5, 8, new[] { 9 }),

                // 4
                ("IN202211004567", "Biodegradable Composite Polymer Formulation for High-Barrier Food Packaging",
                 "A compostable bio-polymer blend derived from agricultural residue exhibiting moisture and oxygen barrier properties comparable to petroleum-based plastics.",
                 new DateTime(2022, 5, 12), new DateTime(2023, 11, 12), new DateTime(2024, 4, 18), new DateTime(2042, 5, 12), "Granted", 1, 2, new[] { 3 }),

                // 5
                ("IN202411005678", "Targeted Nano-Carrier Drug Delivery System for Solid Oncology Tumors",
                 "Liposomal nano-particles functionalized with dual ligands for pH-triggered controlled release directly within tumor microenvironments.",
                 new DateTime(2024, 6, 1), null, null, new DateTime(2044, 6, 1), "Filed", 4, 6, new[] { 7 }),

                // 6
                ("IN202311006789", "Adaptive Torque Vectoring Algorithm for Multi-Motor Electric Commercial Vehicles",
                 "A regenerative braking and dynamic torque allocation control system improving stability and energy recuperation on slippery road surfaces.",
                 new DateTime(2023, 8, 14), new DateTime(2025, 2, 14), null, new DateTime(2043, 8, 14), "Under Examination", 9, 13, new[] { 14 }),

                // 7
                ("IN202111007890", "Autonomous Robotic Manipulator Gripper with Tactile Haptic Feedback Array",
                 "A soft robotics pneumatic gripper equipped with piezoelectric sensory matrix for high-speed fragile item sorting in fulfillment warehouses.",
                 new DateTime(2021, 9, 5), new DateTime(2023, 3, 5), new DateTime(2023, 12, 1), new DateTime(2041, 9, 5), "Granted", 6, 10, new int[0]),

                // 8
                ("IN202511008901", "Zero-Knowledge Proof Authentication Framework for Microservice Architectures",
                 "A cryptographic access-delegation protocol validating client claims across distributed service meshes without exposing credential data.",
                 new DateTime(2025, 1, 20), null, null, new DateTime(2045, 1, 20), "Draft", 8, 12, new[] { 4 }),

                // 9
                ("IN202211009012", "Sub-2nm FinFET Transistor Channel Isolation via Atomic Layer Deposition",
                 "A semiconductor manufacturing process utilizing selective dielectric deposition to prevent parasitic leakage in gate-all-around field effect devices.",
                 new DateTime(2022, 11, 30), new DateTime(2024, 5, 30), new DateTime(2024, 10, 25), new DateTime(2042, 11, 30), "Granted", 7, 11, new int[0]),

                // 10
                ("IN202411010123", "Automated Contract Vulnerability Scanner for Cross-Chain Smart Contracts",
                 "A symbolic execution engine analyzing EVM bytecode for reentrancy, integer overflow, and flash-loan exploitation vulnerabilities.",
                 new DateTime(2024, 4, 5), null, null, new DateTime(2044, 4, 5), "Filed", 3, 4, new[] { 12 }),

                // 11
                ("IN202011011234", "High-Throughput CRISPR-Cas12 Screening Method for Viral Pathogen Detection",
                 "An isothermal nucleic acid amplification assay coupled with fluorescent reporter cleavage for rapid point-of-care infectious disease diagnostics.",
                 new DateTime(2020, 4, 10), new DateTime(2021, 10, 10), new DateTime(2022, 8, 14), new DateTime(2040, 4, 10), "Granted", 4, 6, new[] { 7 }),

                // 12
                ("IN201911012345", "Fault-Tolerant Sharding Scheme for High-Concurrency Distributed Ledgers",
                 "A probabilistic consensus algorithm ensuring Byzantine fault tolerance across heterogeneous validator clusters.",
                 new DateTime(2019, 7, 18), new DateTime(2021, 1, 18), null, new DateTime(2039, 7, 18), "Rejected", 2, 4, new int[0]),

                // 13
                ("IN200311013456", "Legacy Circuit Topology for Variable Frequency Induction Motor Drives",
                 "A pulse-width modulation inverter topology reducing harmonic distortion in three-phase industrial induction motors.",
                 new DateTime(2003, 5, 10), new DateTime(2004, 11, 10), new DateTime(2005, 9, 12), new DateTime(2023, 5, 10), "Expired", 0, 0, new int[0]),

                // 14
                ("IN202411014567", "Solid-State Sodium-Ion Battery Electrolyte with Enhanced Thermal Stability",
                 "A ceramic-polymer composite electrolyte offering ionic conductivity above 1 mS/cm at ambient temperatures with non-flammable properties.",
                 new DateTime(2024, 3, 11), null, null, new DateTime(2044, 3, 11), "Filed", 9, 13, new[] { 2 }),

                // 15
                ("IN202311015678", "Decentralized Federated Learning on Homomorphically Encrypted Healthcare Data",
                 "A privacy-preserving model training protocol allowing hospital nodes to collaborate on diagnostic AI without pooling patient clinical records.",
                 new DateTime(2023, 10, 22), new DateTime(2025, 4, 22), null, new DateTime(2043, 10, 22), "Under Examination", 0, 0, new[] { 12, 6 }),

                // 16
                ("IN202211016789", "Smart Grid Dynamic Load Balancing via Bidirectional Electric Vehicle Charging",
                 "A Vehicle-to-Grid (V2G) scheduling algorithm mitigating peak transformer stress while guaranteeing target state-of-charge for vehicle owners.",
                 new DateTime(2022, 8, 19), new DateTime(2024, 2, 19), new DateTime(2024, 11, 5), new DateTime(2042, 8, 19), "Granted", 5, 9, new[] { 13 }),

                // 17
                ("IN202411017890", "Reinforcement Learning Flight Controller for Multi-Rotor Drone Gust Rejection",
                 "An adaptive aerodynamic attitude control policy trained via neural network simulation to maintain station keeping during extreme turbulence.",
                 new DateTime(2024, 7, 8), null, null, new DateTime(2044, 7, 8), "Under Examination", 6, 10, new[] { 0 }),

                // 18
                ("IN202111018901", "Microfluidic Lab-on-a-Chip for Continuous Circulating Tumor Cell Isolation",
                 "A passive hydrodynamic cell separation device utilizing inertial focusing channels for label-free capture of rare metastatic cells.",
                 new DateTime(2021, 12, 4), new DateTime(2023, 6, 4), new DateTime(2024, 1, 20), new DateTime(2041, 12, 4), "Granted", 4, 7, new[] { 6 }),

                // 19
                ("IN202511019012", "Optical Neural Accelerator Utilizing Silicon Photonic Ring Resonators",
                 "A photonic tensor core executing matrix-vector multiplications at the speed of light using wavelength division multiplexed laser pulses.",
                 new DateTime(2025, 2, 14), null, null, new DateTime(2045, 2, 14), "Draft", 7, 11, new[] { 0 }),

                // 20
                ("IN202311020123", "Self-Healing Anti-Corrosion Nanocoating for Marine Offshore Structures",
                 "A polyurea matrix embedding microencapsulated corrosion inhibitors that rupture and seal microcracks upon mechanical impact.",
                 new DateTime(2023, 4, 17), new DateTime(2024, 10, 17), null, new DateTime(2043, 4, 17), "Published", 1, 3, new[] { 2 }),

                // 21
                ("IN202211021234", "Context-Aware Automated Code Refactoring Engine for Enterprise Monoliths",
                 "A program synthesis system extracting bounded contexts from legacy codebases and generating containerized RESTful microservices.",
                 new DateTime(2022, 3, 29), new DateTime(2023, 9, 29), new DateTime(2024, 6, 12), new DateTime(2042, 3, 29), "Granted", 2, 4, new[] { 0 }),

                // 22
                ("IN202011022345", "Bifacial Solar Module Mount with Autonomous Cleaning and Solar Tracking",
                 "A dual-axis solar tracking armature with integrated electrostatic dust-sweeping wipers powered directly from auxiliary parasitic cells.",
                 new DateTime(2020, 9, 15), new DateTime(2022, 3, 15), null, new DateTime(2040, 9, 15), "Abandoned", 5, 8, new int[0]),

                // 23
                ("IN202411023456", "Hardware Root of Trust with Physical Unclonable Function (PUF) Silicon Key",
                 "An on-chip cryptographic key generation system leveraging manufacturing silicon lattice variations for tamper-proof device identity.",
                 new DateTime(2024, 9, 10), null, null, new DateTime(2044, 9, 10), "Filed", 8, 12, new[] { 11 }),

                // 24
                ("IN202311024567", "Continuous Blood Glucose Sensor with Non-Enzymatic Nanoporous Platinum Probe",
                 "An implantable electrochemical subcutaneous sensor providing direct glucose oxidation current without requiring glucose oxidase enzymes.",
                 new DateTime(2023, 11, 5), new DateTime(2025, 5, 5), null, new DateTime(2043, 11, 5), "Under Examination", 4, 7, new[] { 6 }),

                // 25
                ("IN202111025678", "Direct-Drive In-Wheel Hub Motor with Integrated Liquid Cooling Channels",
                 "A brushless DC axial flux motor design fitted into passenger vehicle wheel hubs with helical stator coolant jackets.",
                 new DateTime(2021, 6, 20), new DateTime(2022, 12, 20), new DateTime(2023, 9, 10), new DateTime(2041, 6, 20), "Granted", 9, 14, new[] { 13 }),

                // 26
                ("IN202411026789", "Collaborative Assembly Robot with Computer Vision Collision Avoidance",
                 "A safety-certified cobot operating alongside human technicians, adjusting velocity profiles dynamically based on 3D depth camera field monitoring.",
                 new DateTime(2024, 5, 25), null, null, new DateTime(2044, 5, 25), "Filed", 6, 10, new[] { 5 }),

                // 27
                ("IN202311027890", "Multi-Modal Document Parsing Engine for Complex Legal and Financial Tables",
                 "A hybrid vision-transformer model understanding non-standard tabular structures, cross-page disclosures, and nested footnotes.",
                 new DateTime(2023, 7, 30), new DateTime(2025, 1, 30), null, new DateTime(2043, 7, 30), "Published", 3, 5, new[] { 4 }),

                // 28
                ("IN200411028901", "Early Generation Analog Phase-Locked Loop for Baseband Radio Frequencies",
                 "A voltage-controlled oscillator circuit achieving rapid frequency lock in dual-band cellular communications equipment.",
                 new DateTime(2004, 2, 14), new DateTime(2005, 8, 14), new DateTime(2006, 4, 10), new DateTime(2024, 2, 14), "Expired", 7, 11, new int[0]),

                // 29
                ("IN202411029012", "Quantum Key Distribution Repeater Node with Entanglement Swapping",
                 "A photon storage node extending secure quantum communications distance beyond optical fiber attenuation limits.",
                 new DateTime(2024, 8, 1), null, null, new DateTime(2044, 8, 1), "Under Examination", 8, 12, new[] { 11 }),

                // 30
                ("IN202511030123", "Predictive Battery Health Diagnostics using Impedance Spectroscopy & Neural Nets",
                 "A battery management firmware calculating lithium plating risk and remaining useful life by applying high-frequency AC perturbation waveforms.",
                 new DateTime(2025, 3, 1), null, null, new DateTime(2045, 3, 1), "Draft", 9, 13, new[] { 14, 0 }),

                // 31
                ("IN202211031234", "High-Resolution Seismic Data Reconstruction via Generative Adversarial Networks",
                 "A geological geophysical modeling system extrapolating subsurface rock strata continuity from sparse offshore sensor geophone traces.",
                 new DateTime(2022, 10, 10), new DateTime(2024, 4, 10), new DateTime(2025, 1, 15), new DateTime(2042, 10, 10), "Granted", 2, 5, new[] { 0 }),

                // 32
                ("IN202311032345", "Superhydrophobic Textile Treatment via Plasma Enhanced Chemical Vapor Deposition",
                 "A fluorocarbon-free surface functionalization imparting extreme oil and water repellency to industrial synthetic fabrics.",
                 new DateTime(2023, 9, 18), new DateTime(2025, 3, 18), null, new DateTime(2043, 9, 18), "Under Examination", 1, 3, new[] { 2 })
            };

            foreach (var item in patentData)
            {
                var patent = new Patent
                {
                    PatentNumber = item.num,
                    Title = item.title,
                    Abstract = item.abs,
                    FilingDate = item.filed,
                    PublicationDate = item.pub,
                    GrantDate = item.grant,
                    ExpiryDate = item.exp,
                    StatusId = statusMap[item.status],
                    ClientId = clients[item.clientIdx].ClientId,
                    CreatedAt = item.filed
                };

                // Add primary inventor
                patent.PatentInventors.Add(new PatentInventor
                {
                    InventorId = inventors[item.primaryInvIdx].InventorId,
                    IsPrimaryInventor = true
                });

                // Add co-inventors
                foreach (var coIdx in item.coInvIdxs)
                {
                    if (coIdx != item.primaryInvIdx)
                    {
                        patent.PatentInventors.Add(new PatentInventor
                        {
                            InventorId = inventors[coIdx].InventorId,
                            IsPrimaryInventor = false
                        });
                    }
                }

                // Add sample document metadata
                patent.Documents.Add(new PatentDocument
                {
                    DocumentName = $"{item.num}_Form1_Application.pdf",
                    DocumentType = "Application",
                    FilePath = $"uploads/{item.num}_Form1_Application.pdf",
                    FileSize = 1024 * 342,
                    UploadedAt = item.filed.AddDays(2),
                    UploadedBy = "admin@ipvault.com"
                });

                patent.Documents.Add(new PatentDocument
                {
                    DocumentName = $"{item.num}_Form2_Complete_Specification.pdf",
                    DocumentType = "Specification",
                    FilePath = $"uploads/{item.num}_Form2_Complete_Specification.pdf",
                    FileSize = 1024 * 1480,
                    UploadedAt = item.filed.AddDays(3),
                    UploadedBy = "attorney@ipvault.com"
                });

                if (item.status == "Granted")
                {
                    patent.Documents.Add(new PatentDocument
                    {
                        DocumentName = $"{item.num}_Letters_Patent_Certificate.pdf",
                        DocumentType = "Certificate",
                        FilePath = $"uploads/{item.num}_Letters_Patent_Certificate.pdf",
                        FileSize = 1024 * 512,
                        UploadedAt = item.grant ?? item.filed.AddYears(2),
                        UploadedBy = "attorney@ipvault.com"
                    });
                }

                await context.Patents.AddAsync(patent);
            }

            await context.SaveChangesAsync();
        }
    }
}
