#include <iostream>
#include <vector>
#include <string>
#include <sstream>
#include <cmath>
#include <map>

// Represents a standard 3SAT or CNF Clause (Vector of non-zero integer literals)
using Clause = std::vector<int>;

// Maps Variable IDs to their current Boolean assignment state (true/false)
using VariableAssignment = std::map<int, bool>;

// Holds the high-performance memory structure of parsed logic constraints
struct LogicConstraintMatrix {
    int num_variables = 0;
    int num_clauses = 0;
    std::vector<Clause> clause_list;

    void PrintMatrixSummary() const {
        std::cout << "[C++ Logic Matrix Initialization Summary]\n"
                  << "  Total Registered Variables: " << num_variables << "\n"
                  << "  Total Ingested Clauses:   " << num_clauses << "\n";
    }
};

class DimacsParser {
public:
    /**
     * Parses raw JSON/DIMACS block data directly into the structural C++ core logic loop.
     */
    static LogicConstraintMatrix IngestRawClauses(const std::vector<std::vector<int>>& raw_clauses) {
        LogicConstraintMatrix matrix;
        matrix.num_clauses = raw_clauses.size();
        
        int max_var_id = 0;

        for (const auto& raw_clause : raw_clauses) {
            Clause formatted_clause;
            for (int literal : raw_clause) {
                if (literal != 0) {
                    formatted_clause.push_back(literal);
                    int var_id = std::abs(literal);
                    if (var_id > max_var_id) {
                        max_var_id = var_id;
                    }
                }
            }
            matrix.clause_list.push_back(formatted_clause);
        }

        matrix.num_variables = max_var_id;
        return matrix;
    }
};

class VerificationHarness {
public:
    /**
     * Performs a deterministic pass over the matrix to evaluate if the 
     * given state assignments completely satisfy every input clause.
     */
    static bool VerifySolution(const LogicConstraintMatrix& matrix, const VariableAssignment& assignment) {
        std::cout << "\n[Harness Execution Triggered]: Evaluating CNF Satisfiability Matrix...\n";
        
        for (size_t i = 0; i < matrix.clause_list.size(); ++i) {
            const auto& clause = matrix.clause_list[i];
            bool clause_satisfied = false;

            for (int literal : clause) {
                int var_id = std::abs(literal);
                bool is_negative = (literal < 0);

                // Safe fallback check if a variable index isn't mapped
                if (assignment.find(var_id) == assignment.end()) {
                    std::cout << "  [Harness Warning]: Variable " << var_id << " is missing an assignment state.\n";
                    continue; 
                }

                bool current_truth = assignment.at(var_id);
                // Invert the truth evaluation if the literal maps to a negative clause token (~X)
                bool literal_evaluation = is_negative ? !current_truth : current_truth;

                if (literal_evaluation) {
                    clause_satisfied = true;
                    break; // An OR clause requires only one true literal to satisfy the block
                }
            }

            // Short-circuit checking if any structural rule fails
            if (!clause_satisfied) {
                std::cout << "  [Verification Failed]: Clause #" << (i + 1) << " evaluates to FALSE.\n";
                return false;
            }
        }

        std::cout << "  [Verification Success]: 100% of clauses satisfied. Structural equilibrium holding.\n";
        return true;
    }
};

// =============================================================================
// RUNTIME HARDWARE / SIMULATION LAYER EXECUTION
// =============================================================================
int main() {
    std::cout << "--- Initializing Low-Level C++ GCC Logic Engine ---\n\n";

    // 1. Ingest sample structural clauses from Python Curator Engine
    // Mapped from physical system conditions: [[~SteelConduit_01_Fe_Alpha, SteelConduit_01_Fe_Beta], ...]
    std::vector<std::vector<int>> external_json_clauses = {
        {-1, 2}, // Clause 1: (~Fe_Alpha OR Fe_Beta)
        {-2, 3}  // Clause 2: (~Fe_Beta OR C_Interstitial)
    };

    LogicConstraintMatrix dynamic_matrix = DimacsParser::IngestRawClauses(external_json_clauses);
    dynamic_matrix.PrintMatrixSummary();

    // 2. Test Configuration A: An unstable material environment configuration
    // (Fe_Alpha is active, but Fe_Beta breaks down)
    VariableAssignment unstable_assignment = {
        {1, true},   // Fe_Alpha active
        {2, false},  // Fe_Beta collapsed
        {3, true}    // C_Interstitial active
    };
    
    std::cout << "\n--- Testing Configuration A (Simulated Environmental Strain) ---";
    bool config_a_result = VerificationHarness::VerifySolution(dynamic_matrix, unstable_assignment);
    std::cout << "  Configuration Result -> System Stable: " << (config_a_result ? "TRUE" : "FALSE") << "\n";

    // 3. Test Configuration B: A perfectly balanced variable state array
    VariableAssignment stable_assignment = {
        {1, true},   // Fe_Alpha active
        {2, true},   // Fe_Beta active
        {3, true}    // C_Interstitial active
    };

    std::cout << "\n--- Testing Configuration B (System Equilibrium) ---";
    bool config_b_result = VerificationHarness::VerifySolution(dynamic_matrix, stable_assignment);
    std::cout << "  Configuration Result -> System Stable: " << (config_b_result ? "TRUE" : "FALSE") << "\n";

    std::cout << "\n--- C++ Execution Matrix Successfully Armed for 3SAT Core Ingestion ---\n";
    return 0;
}
